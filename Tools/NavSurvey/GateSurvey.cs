using Andre.IO.VFS;
using Havok.hk2018;
using SoulsFormats;
using StudioCore.Editors.MapEditor;
using System.Numerics;
using System.Text.RegularExpressions;

// gatesurvey [regex] [max] : how the game's NVA gate nodes relate to the navmeshes and their connections, to settle
// the rules a gate rebuild must follow (Navigation Editor Plan, Gate nodes):
// - how many gates a navmesh has per connected neighbour, and per separate run of connected edges to it;
// - where a gate sits: against its connection edges, the run's middle, its paired gate, and the faces;
// - what a cost is: against the straight distance between the two gates, and the walking distance over the faces;
// - what the unpaired gates (NodeSubID -1) have in common.
// Geometry is in each mesh's own space, which is map space for the entries without a rotation; rotated entries are
// counted and left out of the geometry.
static class GateSurvey
{
    public static void Run(VirtualFileSystem fs, string dict, string[] args)
    {
        var re = new Regex(args.Length > 1 ? args[1] : ".");
        int max = args.Length > 2 ? int.Parse(args[2]) : 2000;
        var maps = Regex.Matches(dict, @"""/map/m\d\d/(m\d\d_\d\d_\d\d_\d\d)/\1\.nva\.dcx""").Select(m => m.Groups[1].Value)
            .Distinct().Where(m => re.IsMatch(m)).Take(max).ToList();
        var profile = Havok.HavokProfile.FromGame("ER");
        var c = new Dictionary<string, long>();
        var examples = new Dictionary<string, string>();
        void Count(string key, long n = 1, string example = null) { c[key] = c.GetValueOrDefault(key) + n; if (example != null) examples.TryAdd(key, example); }
        var stats = new Dictionary<string, List<float>>();
        void Stat(string key, float v) { if (!stats.TryGetValue(key, out var l)) stats[key] = l = new(); l.Add(v); }

        foreach (var mapId in maps)
        {
            var map = NavigationReader.Read(fs, mapId, profile, Array.Empty<string>());
            var nva = map?.Nva;
            if (nva == null) continue;
            var list = nva.Navmeshes;
            Count("maps");
            var index = list.Select((e, i) => (e, i)).ToDictionary(x => x.e, x => x.i);
            var byNameId = new Dictionary<int, NVA_ER.Navmesh>();
            foreach (var e in list) byNameId.TryAdd(e.NameID, e);

            // The connection edges of each ordered pair (owner, neighbour), on the owner's mesh
            var pairEdges = new Dictionary<(int, int), List<int>>();
            var pairConns = new Dictionary<(int, int), List<NVA_ER.NavMeshConnection>>();
            foreach (var k in nva.Connectors)
            {
                if (!pairEdges.TryGetValue((k.MainNameID, k.TargetNameID), out var edges)) pairEdges[(k.MainNameID, k.TargetNameID)] = edges = new();
                edges.AddRange(k.NavMeshConnections.Select(n => n.EdgeIndex));
                if (!pairConns.TryGetValue((k.MainNameID, k.TargetNameID), out var conns)) pairConns[(k.MainNameID, k.TargetNameID)] = conns = new();
                conns.AddRange(k.NavMeshConnections);
            }

            foreach (var owner in list)
            {
                int ownerIndex = index[owner];
                var mesh = map.FileOf(owner)?.Mesh;
                bool rotated = owner.Rotation != Vector3.Zero || owner.Position != Vector3.Zero;
                bool geometry = mesh != null && mesh.m_faces.Count > 0 && !rotated;
                var set = NavigationMap.SetOf(owner);
                string s = set.ToString();

                var neighbours = pairEdges.Keys.Where(p => p.Item1 == owner.NameID).Select(p => p.Item2).Distinct().ToList();
                var gates = owner.GateNodes;
                Count($"navmeshes {s}");
                if (gates.Count > 0) Count($"navmeshes {s} with gates");
                if (rotated && gates.Count > 0) Count("navmeshes with gates, rotated (no geometry)");
                Count($"gates per navmesh {s}: {Bucket(gates.Count)}");
                Count($"connected neighbours per navmesh {s}: {Bucket(neighbours.Count)}");

                // Gates per neighbour
                var gatesTo = gates.GroupBy(g => (int)g.ConnectedNavmeshIndex).ToDictionary(g => g.Key, g => g.Count());
                foreach (var nb in neighbours)
                {
                    int nbIndex = byNameId.TryGetValue(nb, out var nbEntry) ? index[nbEntry] : -1;
                    int count = gatesTo.GetValueOrDefault(nbIndex);
                    Count($"neighbour {s}: gates to it {Bucket(count)}", 1, $"{mapId} {owner.NameID}->{nb}");

                    if (!geometry) continue;
                    var runs = Runs(mesh, pairEdges[(owner.NameID, nb)]);
                    Count($"neighbour {s}: runs {Bucket(runs.Count)}, gates {Bucket(count)}", 1, $"{mapId} {owner.NameID}->{nb}");
                    if (count > 0) Count(count == runs.Count ? $"neighbour {s} with gates: gates = runs" : count < runs.Count ? $"neighbour {s} with gates: gates < runs" : $"neighbour {s} with gates: gates > runs");
                    foreach (var gap in new[] { 0.5f, 2f, 5f })
                    {
                        var loose = Runs(mesh, pairEdges[(owner.NameID, nb)], gap);
                        if (count > 0) Count(count == loose.Count ? $"neighbour {s} with gates: gates = runs joined within {gap} m" : count < loose.Count ? $"neighbour {s} with gates: gates < runs joined within {gap} m" : $"neighbour {s} with gates: gates > runs joined within {gap} m", 1, $"{mapId} {owner.NameID}->{nb} {count} vs {loose.Count}");
                    }
                    if (count == 0) Stat($"neighbour {s} without gates: connected length (m)", runs.Sum(r => r.Length));
                    else Stat($"neighbour {s} with gates: connected length (m)", runs.Sum(r => r.Length));
                }
                foreach (var (target, n) in gatesTo)
                {
                    if (target < 0 || target >= list.Count) continue;
                    if (!neighbours.Contains(list[target].NameID)) Count($"gate {s}: to a navmesh with no connection", n, $"{mapId} {owner.NameID}");
                }

                // Where each gate sits
                for (int g = 0; g < gates.Count; g++)
                {
                    var gate = gates[g];
                    int t = gate.ConnectedNavmeshIndex;
                    Count($"gates {s}");
                    if (t < 0 || t >= list.Count) { Count($"gate {s}: index out of the list"); continue; }
                    var target = list[t];

                    // Unpaired
                    if (gate.NodeSubID < 0)
                    {
                        Count($"gate {s}: unpaired");
                        var back = target.GateNodes.Count(x => x.ConnectedNavmeshIndex == ownerIndex);
                        Count($"gate {s} unpaired: the target has {Bucket(back)} gates back, {Bucket(target.GateNodes.Count)} in all", 1, $"{mapId} {owner.NameID}#{g}->{target.NameID}");
                        Count($"gate {s} unpaired: target {(target.FaceCount == 0 ? "a placeholder" : "with faces")}, set {NavigationMap.SetOf(target)}");
                        Count($"gate {s} unpaired: the owner has {Bucket(gates.Count)} gates");
                        Count($"gate {s} unpaired: {(target.GateNodes.Count >= 16 ? "the target has 16 gates (full)" : gates.Count >= 16 ? "the owner has 16 gates (full)" : "neither has 16 gates")}", 1, $"{mapId} {owner.NameID}#{g}->{target.NameID} ({gates.Count}, {target.GateNodes.Count})");
                    }
                    if (gate.NodeSubID >= 0 && gate.NodeSubID >= target.GateNodes.Count) Count($"gate {s}: sub ID past the target's gates");
                    if (gates.Count == 16 || target.GateNodes.Count == 16)
                    {
                        Count($"gate {s} where a side is full (16): {(gate.NodeSubID < 0 ? "unpaired" : "paired")}");
                    }
                    if (gate.NodeSubID >= 0 && gate.NodeSubID < target.GateNodes.Count)
                    {
                        var partner = target.GateNodes[gate.NodeSubID];
                        Stat($"gate {s}: distance to its paired gate (m)", Vector3.Distance(gate.Position, partner.Position));
                    }

                    if (!geometry || !pairEdges.TryGetValue((owner.NameID, target.NameID), out var cEdges)) continue;
                    var runs = Runs(mesh, cEdges);
                    var p = gate.Position;

                    float nearestEdge = cEdges.Select(e => SegmentDistance(p, V(mesh, mesh.m_edges[e].m_a), V(mesh, mesh.m_edges[e].m_b))).DefaultIfEmpty(float.MaxValue).Min();
                    Stat($"gate {s}: distance to its nearest connection edge (m)", nearestEdge);
                    int ne = cEdges.OrderBy(e => SegmentDistance(p, V(mesh, mesh.m_edges[e].m_a), V(mesh, mesh.m_edges[e].m_b))).First();
                    var ea = V(mesh, mesh.m_edges[ne].m_a); var eb = V(mesh, mesh.m_edges[ne].m_b);
                    Stat($"gate {s}: distance to its nearest connection edge's middle (m)", Vector3.Distance(p, (ea + eb) / 2));
                    float along = Vector3.Distance(ea, eb) < 1e-4f ? 0 : Vector3.Distance(ea, Closest(p, ea, eb)) / Vector3.Distance(ea, eb);
                    Count($"gate {s}: along its nearest connection edge {(along < 0.02f ? "at a" : along > 0.98f ? "at b" : Math.Abs(along - 0.5f) < 0.02f ? "at the middle" : "elsewhere")}");
                    Stat($"gate {s}: nearest connection edge length (m)", Vector3.Distance(ea, eb));

                    // The overlap of the edge with the neighbour's edge it is joined to: its middle, and whether the
                    // gate's edge is the one with the longest overlap to the target
                    var targetMesh = map.FileOf(target)?.Mesh;
                    bool targetRotated = target.Rotation != Vector3.Zero || target.Position != Vector3.Zero;
                    if (targetMesh != null && !targetRotated && pairConns.TryGetValue((owner.NameID, target.NameID), out var conns))
                    {
                        (Vector3 Mid, float Length)? Overlap(NVA_ER.NavMeshConnection n)
                        {
                            if (n.EdgeIndex < 0 || n.EdgeIndex >= mesh.m_edges.Count || n.OppositeEdgeIndex < 0 || n.OppositeEdgeIndex >= targetMesh.m_edges.Count) return null;
                            Vector3 a0 = V(mesh, mesh.m_edges[n.EdgeIndex].m_a), a1 = V(mesh, mesh.m_edges[n.EdgeIndex].m_b);
                            Vector3 b0 = V(targetMesh, targetMesh.m_edges[n.OppositeEdgeIndex].m_a), b1 = V(targetMesh, targetMesh.m_edges[n.OppositeEdgeIndex].m_b);
                            var ab = a1 - a0; float l2 = ab.LengthSquared();
                            if (l2 < 1e-10f) return null;
                            float t0 = Math.Clamp(Vector3.Dot(b0 - a0, ab) / l2, 0, 1), t1 = Math.Clamp(Vector3.Dot(b1 - a0, ab) / l2, 0, 1);
                            float lo = Math.Min(t0, t1), hi = Math.Max(t0, t1);
                            return (a0 + ab * ((lo + hi) / 2), (hi - lo) * MathF.Sqrt(l2));
                        }
                        var own = conns.Where(n => n.EdgeIndex == ne).Select(Overlap).Where(o => o != null).Select(o => o.Value).ToList();
                        if (own.Count > 0)
                        {
                            float best = own.Min(o => Vector3.Distance(p, o.Mid));
                            Stat($"gate {s}: distance to the middle of its edge's overlap with the neighbour's edge (m)", best);
                            Count($"gate {s}: at its edge's overlap middle {(best < 0.01f ? "(within 1 cm)" : best < 0.1f ? "(within 10 cm)" : "(further)")}");
                            if (best >= 0.01f && Vector3.Distance(p, (ea + eb) / 2) < 0.01f) Count($"gate {s}: at the edge middle but not the overlap middle");
                            var all = conns.Select(n => (n.EdgeIndex, O: Overlap(n))).Where(x => x.O != null).ToList();
                            var longest = all.OrderByDescending(x => x.O.Value.Length).First();
                            Count(longest.EdgeIndex == ne ? $"gate {s}: on the edge with the longest overlap to the target" : $"gate {s}: not on the edge with the longest overlap");
                            // Within its own run (2 m), the longest overlap
                            var run2 = Runs(mesh, cEdges, 2f).First(r => r.Edges.Contains(ne));
                            var inRun = all.Where(x => run2.Edges.Contains(x.EdgeIndex)).OrderByDescending(x => x.O.Value.Length).First();
                            Count(inRun.EdgeIndex == ne ? $"gate {s}: on the longest overlap of its run (2 m)" : $"gate {s}: not on the longest overlap of its run (2 m)");
                        }
                    }
                    Count(cEdges.Distinct().OrderByDescending(e => Vector3.Distance(V(mesh, mesh.m_edges[e].m_a), V(mesh, mesh.m_edges[e].m_b))).First() == ne ? $"gate {s}: on the longest connection edge to the target" : $"gate {s}: not on the longest connection edge");
                    foreach (var gap in new[] { 0.5f, 2f })
                    {
                        var loose = Runs(mesh, cEdges, gap);
                        var lr = loose.OrderBy(r => r.Edges.Min(e => SegmentDistance(p, V(mesh, mesh.m_edges[e].m_a), V(mesh, mesh.m_edges[e].m_b)))).First();
                        Stat($"gate {s}: distance to the length-weighted middle of its run joined within {gap} m", Vector3.Distance(p, lr.Middle));
                        Stat($"gate {s}: distance to the along-the-edge middle of its run joined within {gap} m", Vector3.Distance(p, lr.HalfwayPoint));
                        // The edge point nearest the run's middle: a middle that falls off a bent run, moved onto it
                        var onEdge = ClosestOnSegments(mesh, lr.Edges, lr.Middle);
                        Stat($"gate {s}: distance to the edge point nearest the middle of its run joined within {gap} m", Vector3.Distance(p, onEdge));
                    }
                    var run = runs.OrderBy(r => r.Edges.Min(e => SegmentDistance(p, V(mesh, mesh.m_edges[e].m_a), V(mesh, mesh.m_edges[e].m_b)))).First();
                    Stat($"gate {s}: distance to its run's length-weighted middle (m)", Vector3.Distance(p, run.Middle));
                    Stat($"gate {s}: distance to its run's along-the-edge middle (m)", Vector3.Distance(p, run.HalfwayPoint));
                    Stat($"gate {s}: distance to its run's bounding box centre (m)", Vector3.Distance(p, run.BoxCentre));
                    Stat($"gate {s}: distance to the middle of all connection edges to the target (m)", Vector3.Distance(p, AllMiddle(mesh, cEdges)));
                    var nearestEnd = run.Edges.SelectMany(e => new[] { V(mesh, mesh.m_edges[e].m_a), V(mesh, mesh.m_edges[e].m_b) }).Min(v => Vector3.Distance(p, v));
                    Stat($"gate {s}: distance to its run's nearest vertex (m)", nearestEnd);
                    int face = NearestFace(mesh, p, out float faceDist);
                    Stat($"gate {s}: distance to the nearest face centre (m)", faceDist);
                    Stat($"gate {s}: height above the nearest connection edge (m)", p.Y - ClosestOnSegments(mesh, cEdges, p).Y);
                }

                // Costs
                if (gates.Count == 0) continue;
                List<float[]> walk = null, walkUser = null;
                if (geometry)
                {
                    var starts = gates.Select(g => NearestFace(mesh, g.Position, out _)).ToList();
                    walk = starts.Select(f => Dijkstra(mesh, f, false)).ToList();
                    walkUser = starts.Select(f => Dijkstra(mesh, f, true)).ToList();
                }
                for (int i = 0; i < gates.Count; i++)
                {
                    var costs = gates[i].NeighbourGateNodeCosts;
                    Count($"gate {s}: costs list {(costs.Count == 16 ? "16" : costs.Count.ToString())}");
                    for (int j = 0; j < Math.Min(costs.Count, 16); j++)
                    {
                        short cost = costs[j];
                        if (j >= gates.Count) { Count(cost == -1 ? "cost past the owner's gates: -1" : "cost past the owner's gates: a value"); continue; }
                        if (i == j) { Count(cost == 0 ? "cost to itself: 0" : $"cost to itself: {cost}"); continue; }
                        if (cost < 0)
                        {
                            Count($"cost {s} between two gates: {cost}", 1, $"{mapId} {owner.NameID} {i}->{j}");
                            Count($"cost {s} -1: {(gates[i].ConnectedNavmeshIndex == gates[j].ConnectedNavmeshIndex ? "same" : "different")} target navmesh");
                            if (walk != null)
                            {
                                int fj0 = NearestFace(mesh, gates[j].Position, out _);
                                Count($"cost {s} -1: {(float.IsFinite(walk[i][fj0]) ? "a walk exists" : float.IsFinite(walkUser[i][fj0]) ? "only a walk over user edges" : "no walk")}", 1, $"{mapId} {owner.NameID} {i}->{j}");
                            }
                            continue;
                        }
                        Count($"cost {s} between two gates: a value");

                        var other = gates[j].NeighbourGateNodeCosts;
                        if (i < j && i < other.Count) Count(other[i] == cost ? "cost: symmetric" : Math.Abs(other[i] - cost) <= 1 ? "cost: symmetric within 1" : "cost: not symmetric", 1, $"{mapId} {owner.NameID} {i}<->{j}: {cost} vs {other[i]}");

                        float straight = Vector3.Distance(gates[i].Position, gates[j].Position);
                        if (straight > 0.01f) Stat($"cost {s} / straight distance", cost / straight);
                        Stat($"cost {s} (raw)", cost);
                        if (walk != null)
                        {
                            int fj = NearestFace(mesh, gates[j].Position, out _);
                            float w = walk[i][fj], wu = walkUser[i][fj];
                            if (float.IsFinite(w) && w > 0.01f) Stat($"cost {s} / walking distance (faces)", cost / w);
                            else Count($"cost {s}: no walk between the gates' faces{(float.IsFinite(wu) ? ", but one with user edges" : "")}");
                            if (float.IsFinite(wu) && wu > 0.01f) Stat($"cost {s} / walking distance with user edges", cost / wu);
                            if (float.IsFinite(w) && straight > 0.01f) Stat($"walking / straight distance {s}", w / straight);
                        }
                    }
                }
            }
        }

        foreach (var (k, v) in c.OrderBy(x => x.Key)) Console.WriteLine($"{v,9}  {k}{(examples.TryGetValue(k, out var ex) ? "   e.g. " + ex : "")}");
        Console.WriteLine();
        foreach (var (k, l) in stats.OrderBy(x => x.Key))
        {
            l.Sort();
            float P(double q) => l[(int)Math.Min(l.Count - 1, Math.Floor(q * l.Count))];
            Console.WriteLine($"{k}: n {l.Count}, min {l[0]:0.###}, p10 {P(0.1):0.###}, p50 {P(0.5):0.###}, p90 {P(0.9):0.###}, p99 {P(0.99):0.###}, max {l[^1]:0.###}");
        }
    }

    static string Bucket(int n) => n <= 6 ? n.ToString() : n <= 16 ? "7-16" : ">16";

    static Vector3 V(hkaiNavMesh mesh, int i) { var v = mesh.m_vertices[i]; return new Vector3(v.X, v.Y, v.Z); }

    sealed class EdgeRun
    {
        public List<int> Edges = new();
        public float Length;
        public Vector3 Middle, HalfwayPoint, BoxCentre;
    }

    /// <summary>The connection edges grouped into runs whose edges share an end (within 5 cm).</summary>
    static List<EdgeRun> Runs(hkaiNavMesh mesh, List<int> edges, float gap = 0.05f)
    {
        var distinct = edges.Distinct().Where(e => e >= 0 && e < mesh.m_edges.Count).ToList();
        var parent = distinct.ToDictionary(e => e, e => e);
        int Find(int x) { while (parent[x] != x) x = parent[x] = parent[parent[x]]; return x; }
        for (int i = 0; i < distinct.Count; i++)
            for (int j = i + 1; j < distinct.Count; j++)
            {
                var a = mesh.m_edges[distinct[i]]; var b = mesh.m_edges[distinct[j]];
                // Joined when an end of either edge is within the gap of the other edge
                Vector3 a0 = V(mesh, a.m_a), a1 = V(mesh, a.m_b), b0 = V(mesh, b.m_a), b1 = V(mesh, b.m_b);
                float d = Math.Min(Math.Min(SegmentDistance(a0, b0, b1), SegmentDistance(a1, b0, b1)), Math.Min(SegmentDistance(b0, a0, a1), SegmentDistance(b1, a0, a1)));
                if (d < gap)
                    parent[Find(distinct[i])] = Find(distinct[j]);
            }

        var runs = new List<EdgeRun>();
        foreach (var group in distinct.GroupBy(Find))
        {
            var run = new EdgeRun { Edges = group.ToList() };
            var sum = Vector3.Zero;
            var min = new Vector3(float.MaxValue); var max = new Vector3(float.MinValue);
            var segs = new List<(Vector3, Vector3, float)>();
            foreach (var e in run.Edges)
            {
                var a = V(mesh, mesh.m_edges[e].m_a); var b = V(mesh, mesh.m_edges[e].m_b);
                float len = Vector3.Distance(a, b);
                run.Length += len; sum += (a + b) / 2 * len;
                min = Vector3.Min(min, Vector3.Min(a, b)); max = Vector3.Max(max, Vector3.Max(a, b));
                segs.Add((a, b, len));
            }
            run.Middle = run.Length > 0 ? sum / run.Length : (min + max) / 2;
            run.BoxCentre = (min + max) / 2;

            // Halfway along the run, walking its edges in order from one end (edges sorted along the run's long axis)
            var axis = max - min;
            Func<Vector3, float> key = axis.X >= axis.Z ? v => v.X : v => v.Z;
            segs = segs.OrderBy(x => Math.Min(key(x.Item1), key(x.Item2))).ToList();
            float half = run.Length / 2, walked = 0;
            run.HalfwayPoint = run.Middle;
            foreach (var (a, b, len) in segs)
            {
                if (walked + len >= half && len > 0)
                {
                    var (from, to) = key(a) <= key(b) ? (a, b) : (b, a);
                    run.HalfwayPoint = Vector3.Lerp(from, to, (half - walked) / len);
                    break;
                }
                walked += len;
            }
            runs.Add(run);
        }
        return runs;
    }

    static Vector3 AllMiddle(hkaiNavMesh mesh, List<int> edges)
    {
        var sum = Vector3.Zero; float total = 0;
        foreach (var e in edges.Distinct())
        {
            var a = V(mesh, mesh.m_edges[e].m_a); var b = V(mesh, mesh.m_edges[e].m_b);
            float len = Vector3.Distance(a, b); sum += (a + b) / 2 * len; total += len;
        }
        return total > 0 ? sum / total : sum;
    }

    static Vector3 Closest(Vector3 p, Vector3 a, Vector3 b)
    {
        var ab = b - a; float d = ab.LengthSquared();
        float t = d < 1e-12f ? 0 : Math.Clamp(Vector3.Dot(p - a, ab) / d, 0, 1);
        return a + ab * t;
    }

    static float SegmentDistance(Vector3 p, Vector3 a, Vector3 b) => Vector3.Distance(p, Closest(p, a, b));

    static Vector3 ClosestOnSegments(hkaiNavMesh mesh, List<int> edges, Vector3 p)
        => edges.Select(e => Closest(p, V(mesh, mesh.m_edges[e].m_a), V(mesh, mesh.m_edges[e].m_b))).OrderBy(q => Vector3.Distance(p, q)).First();

    static int NearestFace(hkaiNavMesh mesh, Vector3 p, out float distance)
    {
        int best = 0; distance = float.MaxValue;
        for (int f = 0; f < mesh.m_faces.Count; f++)
        {
            float d = Vector3.Distance(p, NavigationOverlayBuilder.FaceCentre(mesh, f));
            if (d < distance) { distance = d; best = f; }
        }
        return best;
    }

    /// <summary>The walking distance from a face's centre to every face's, over original edges (and user edges).</summary>
    static float[] Dijkstra(hkaiNavMesh mesh, int start, bool userEdges)
    {
        int n = mesh.m_faces.Count;
        var centres = new Vector3[n];
        for (int f = 0; f < n; f++) centres[f] = NavigationOverlayBuilder.FaceCentre(mesh, f);
        var dist = Enumerable.Repeat(float.PositiveInfinity, n).ToArray();
        var queue = new PriorityQueue<int, float>();
        dist[start] = 0; queue.Enqueue(start, 0);
        while (queue.TryDequeue(out int f, out float d))
        {
            if (d > dist[f]) continue;
            var face = mesh.m_faces[f];
            IEnumerable<int> Edges()
            {
                for (int k = face.m_startEdgeIndex; k < face.m_startEdgeIndex + face.m_numEdges; k++) yield return k;
                if (userEdges && face.m_numUserEdges > 0)
                    for (int k = face.m_startUserEdgeIndex; k < face.m_startUserEdgeIndex + face.m_numUserEdges; k++) yield return k;
            }
            foreach (var k in Edges())
            {
                var edge = mesh.m_edges[k];
                if (((int)edge.m_flags & (int)hkaiNavMesh.EdgeFlagBits.EDGE_BLOCKED) != 0) continue;
                var o = NavigationOverlayBuilder.OppositeFace(mesh, edge.m_oppositeFace);
                if (o == null) continue;
                float nd = d + Vector3.Distance(centres[f], centres[o.Value]);
                if (nd < dist[o.Value]) { dist[o.Value] = nd; queue.Enqueue(o.Value, nd); }
            }
        }
        return dist;
    }
}
