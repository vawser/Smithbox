using Andre.IO.VFS;
using SoulsFormats;
using StudioCore.Editors.MapEditor;
using System.Numerics;
using System.Text.RegularExpressions;

// stitch [regex] [max] : NavigationStitch against the game's own connections. For in-map (NVA) and cross-map (NVC)
// connections it finds every candidate pair within a loose search, then scores a grid of parameters: how many of the
// game's connections each finds (recall), and how many it adds that the game doesn't have.
static class StitchEval
{
    public static void Run(VirtualFileSystem fs, string dict, string[] args)
    {
        var re = new Regex(args.Length > 1 ? args[1] : ".");
        int max = args.Length > 2 ? int.Parse(args[2]) : 50;
        var maps = Regex.Matches(dict, @"""/map/m\d\d/(m\d\d_\d\d_\d\d_\d\d)/\1\.nva\.dcx""").Select(m => m.Groups[1].Value)
            .Distinct().Where(m => re.IsMatch(m)).Take(max).ToList();
        var nvcNames = Regex.Matches(dict, @"""/map/nvc/(m\d\d_\d\d_\d\d_\d\d_m\d\d_\d\d_\d\d_\d\d)\.nvc\.dcx""").Select(m => m.Groups[1].Value).Distinct().ToList();
        var profile = Havok.HavokProfile.FromGame("ER");
        var loose = new NavStitchParameters { MaxSeparation = 2f, MaxStepHeight = 100f, MinOverlap = 0, MinAlignment = -1 };
        var cache = new Dictionary<string, NavigationMap>();
        NavigationMap Load(string id) => cache.TryGetValue(id, out var m) ? m : cache[id] = NavigationReader.Read(fs, id, profile, NavigationReader.NvcNamesFor(id, nvcNames));

        var kinds = new[] { "nva", "nvc" };
        var hits = kinds.ToDictionary(k => k, _ => new List<NavEdgeFit>());
        var extra = kinds.ToDictionary(k => k, _ => new List<NavEdgeFit>());
        var missed = kinds.ToDictionary(k => k, _ => new Dictionary<string, int>());
        var missedExample = new Dictionary<string, string>();
        var total = kinds.ToDictionary(k => k, _ => 0);
        var extraTags = new Dictionary<string, int>();
        float maxGap = 0;
        var orders = new Dictionary<string, int>();
        void Order(string kind, List<(int Main, int Target, List<NVA_ER.NavMeshConnection> Conns)> list, string where)
        {
            for (int i = 1; i < list.Count; i++)
            {
                var k = list[i - 1].Main.CompareTo(list[i].Main) is int cmp && cmp != 0 ? cmp : list[i - 1].Target.CompareTo(list[i].Target);
                var key = $"{kind}: connectors {(k < 0 ? "ascending" : k == 0 ? "repeated" : "descending")} by (main, target)";
                orders[key] = orders.GetValueOrDefault(key) + 1;
                var ku = ((uint)list[i - 1].Main).CompareTo((uint)list[i].Main) is int cu && cu != 0 ? cu : ((uint)list[i - 1].Target).CompareTo((uint)list[i].Target);
                var keyu = $"{kind}: connectors {(ku < 0 ? "ascending" : ku == 0 ? "repeated" : "descending")} by unsigned (main, target)";
                orders[keyu] = orders.GetValueOrDefault(keyu) + 1;
            }
            foreach (var x in list)
                for (int i = 1; i < x.Conns.Count; i++)
                {
                    var a = x.Conns[i - 1]; var b = x.Conns[i];
                    int k = a.FaceIndex != b.FaceIndex ? a.FaceIndex.CompareTo(b.FaceIndex) : a.EdgeIndex != b.EdgeIndex ? a.EdgeIndex.CompareTo(b.EdgeIndex) : a.OppositeEdgeIndex.CompareTo(b.OppositeEdgeIndex);
                    var key = $"{kind}: connections {(k < 0 ? "ascending" : k == 0 ? "repeated" : "descending")} by (face, edge, opposite edge)";
                    orders[key] = orders.GetValueOrDefault(key) + 1;
                    if (k > 0) missedExample.TryAdd(key, $"{where} {x.Main}->{x.Target} #{i}: ({a.FaceIndex},{a.EdgeIndex},{a.OppositeEdgeIndex}) then ({b.FaceIndex},{b.EdgeIndex},{b.OppositeEdgeIndex})");
                }
        }

        foreach (var mapId in maps)
        {
            var map = Load(mapId);
            if (map?.Nva == null) continue;

            Order("nva", map.Nva.Connectors.Select(x => (x.MainNameID, x.TargetNameID, x.NavMeshConnections)).ToList(), mapId);
            foreach (var nvc in map.Nvcs.Where(n => n.MapA == mapId))
                Order("nvc", nvc.Nvc.Connectors.Select(x => (x.MainNameID, x.TargetNameID, x.NavMeshConnections)).ToList(), nvc.Name);

            foreach (var set in new[] { NavmeshSet.N, NavmeshSet.O })
            {
                var meshes = Meshes(map, set, Vector3.Zero);
                var placed = meshes.ToDictionary(m => m.NameID);
                foreach (var connector in map.Nva.Connectors)
                    if (placed.TryGetValue(connector.MainNameID, out var ma) && placed.TryGetValue(connector.TargetNameID, out var mb))
                        foreach (var c in connector.NavMeshConnections)
                            maxGap = MathF.Max(maxGap, StitchGeometry.Gap(ma, c.EdgeIndex, mb, c.OppositeEdgeIndex));
                var vanilla = new HashSet<(int, int, int, int)>();
                foreach (var connector in map.Nva.Connectors)
                    if (map.FindNavmesh(connector.MainNameID) is { } e && NavigationMap.SetOf(e) == set)
                        foreach (var c in connector.NavMeshConnections)
                            vanilla.Add((connector.MainNameID, c.EdgeIndex, connector.TargetNameID, c.OppositeEdgeIndex));

                Score("nva", vanilla, NavigationStitch.FindWithFit(meshes, loose), mapId);

                // Cross-map, this map as the file's first map
                foreach (var nvc in map.Nvcs.Where(n => n.MapA == mapId))
                {
                    var offset = NavStitchPlanner.TileOffset(nvc.MapA, nvc.MapB);
                    var other = offset == null ? null : Load(nvc.MapB);
                    if (other?.Nva == null) continue;

                    var both = meshes.Concat(Meshes(other, set, offset.Value)).ToList();
                    var nvcVanilla = new HashSet<(int, int, int, int)>();
                    foreach (var connector in nvc.Nvc.Connectors)
                        if (map.FindNavmesh(connector.MainNameID) is { } e && NavigationMap.SetOf(e) == set)
                            foreach (var c in connector.NavMeshConnections)
                                nvcVanilla.Add((connector.MainNameID, c.EdgeIndex, connector.TargetNameID, c.OppositeEdgeIndex));

                    var found = NavigationStitch.FindWithFit(both, loose, (a, b) => a.MapId != b.MapId).Where(x => x.Pair.MapA == mapId).ToList();
                    Score("nvc", nvcVanilla, found, nvc.Name);
                }

                void Score(string kind, HashSet<(int, int, int, int)> vanillaSet, List<(NavStitchPair Pair, NavEdgeFit Fit)> found, string where)
                {
                    total[kind] += vanillaSet.Count;
                    var byKey = new Dictionary<(int, int, int, int), NavEdgeFit>();
                    foreach (var (p, fit) in found) byKey[(p.NameA, p.EdgeA, p.NameB, p.EdgeB)] = fit;

                    foreach (var v in vanillaSet)
                    {
                        if (byKey.TryGetValue(v, out var fit)) hits[kind].Add(fit);
                        else
                        {
                            var why = "outside the search (no overlap, or more than 2 m apart)";
                            missed[kind][why] = missed[kind].GetValueOrDefault(why) + 1;
                            missedExample.TryAdd($"{kind} {why}", $"{where} {v}");
                        }
                    }
                    var connectedEdges = vanillaSet.Select(v => (v.Item1, v.Item2)).ToHashSet();
                    var p0 = new NavStitchParameters();
                    foreach (var (k, fit) in byKey)
                    {
                        if (vanillaSet.Contains(k)) continue;
                        extra[kind].Add(fit);
                        if (!NavigationStitch.Accepts(p0, fit)) continue;
                        bool ca = connectedEdges.Contains((k.Item1, k.Item2)), cb = connectedEdges.Contains((k.Item3, k.Item4));
                        var tag = $"{kind} default-param extra: A edge {(ca ? "connected" : "open")}, B edge {(cb ? "connected" : "open")}, overlap {(fit.Overlap < 0.1f ? "<0.1" : fit.Overlap < 1 ? "<1" : ">=1")}";
                        extraTags[tag] = extraTags.GetValueOrDefault(tag) + 1;
                        missedExample.TryAdd(tag, $"{where} {k}");
                    }
                }
            }
        }

        foreach (var (k, n) in orders.OrderBy(x => x.Key)) Console.WriteLine($"{n,9}  {k}  {missedExample.GetValueOrDefault(k)}");
        Console.WriteLine($"largest gap between the two edges of a game NVA connection: {maxGap:0.###} m");
        foreach (var kind in kinds)
        {
            Console.WriteLine($"== {kind}: {total[kind]} game connections, {hits[kind].Count} within the search, {extra[kind].Count} other candidates");
            foreach (var (why, n) in missed[kind]) Console.WriteLine($"   missed {n}: {why}  e.g. {missedExample.GetValueOrDefault($"{kind} {why}")}");

            Pct("separation", hits[kind].Select(f => f.Separation));
            Pct("step", hits[kind].Select(f => f.Step));
            Pct("overlap", hits[kind].Select(f => f.Overlap));
            Pct("alignment", hits[kind].Select(f => f.Alignment));
            Console.WriteLine($"   alignment min {hits[kind].Min(f => f.Alignment):0.000000}, overlap < 0.02: {hits[kind].Count(f => f.Overlap < 0.02f)}");
            foreach (var (t, n) in extraTags.Where(x => x.Key.StartsWith(kind)).OrderBy(x => x.Key)) Console.WriteLine($"   {n,7} {t}  e.g. {missedExample.GetValueOrDefault(t)}");
            var p0 = new NavStitchParameters();
            Console.WriteLine($"   default parameters: found {hits[kind].Count(f => NavigationStitch.Accepts(p0, f))}, extra {extra[kind].Count(f => NavigationStitch.Accepts(p0, f))}");

            Console.WriteLine("   sep   step  align  minOv |  found  recall  extra");
            var rows = new List<(float, float, float, float, int, int)>();
            foreach (var sep in new[] { 0.01f, 0.05f, 0.1f, 0.25f, 0.5f, 1f, 2f })
                foreach (var step in new[] { 0.1f, 0.5f, 1f, 2f, 5f })
                    foreach (var align in new[] { 0.9f, 0.98f, 0.999f })
                        foreach (var ov in new[] { 0f, 0.02f, 0.1f })
                        {
                            var p = new NavStitchParameters { MaxSeparation = sep, MaxStepHeight = step, MinAlignment = align, MinOverlap = ov };
                            rows.Add((sep, step, align, ov, hits[kind].Count(f => NavigationStitch.Accepts(p, f)), extra[kind].Count(f => NavigationStitch.Accepts(p, f))));
                        }

            // The best trade-offs: for each recall level, the fewest extras
            foreach (var r in rows.OrderByDescending(r => r.Item5 - 2 * r.Item6).Take(12))
                Console.WriteLine($"  {r.Item1,5} {r.Item2,5} {r.Item3,6} {r.Item4,5} | {r.Item5,6} {100.0 * r.Item5 / Math.Max(1, total[kind]),6:0.00}% {r.Item6,6}");
        }
    }

    static void Pct(string name, IEnumerable<float> values)
    {
        var v = values.OrderBy(x => x).ToArray();
        if (v.Length == 0) return;
        string P(double q) => v[(int)Math.Min(v.Length - 1, q * v.Length)].ToString("0.####");
        Console.WriteLine($"   {name}: p50 {P(0.5)} p90 {P(0.9)} p99 {P(0.99)} p99.9 {P(0.999)} max {v[^1]:0.####}");
    }

    /// <summary>A map's meshes of a set with faces, placed at an offset (in-map entries are at the origin in ER).</summary>
    public static List<NavStitchMesh> Meshes(NavigationMap map, NavmeshSet set, Vector3 offset)
        => NavigationOverlayBuilder.EntriesOf(map, set)
            .Select(e => (e, f: map.FileOf(e)))
            .Where(x => x.f?.Mesh != null && x.f.Mesh.m_faces.Count > 0)
            .Select(x => new NavStitchMesh(x.e.NameID, x.f.Mesh, x.e.Position + offset, map.MapId))
            .ToList();
}

static class StitchGeometry
{
    static Vector3 V(Havok.hk2018.hkaiNavMesh m, int i) => new(m.m_vertices[i].X, m.m_vertices[i].Y, m.m_vertices[i].Z);

    /// <summary>The closest distance between two edges, sampled along each.</summary>
    public static float Gap(NavStitchMesh a, int ea, NavStitchMesh b, int eb)
    {
        if (ea >= a.Mesh.m_edges.Count || eb >= b.Mesh.m_edges.Count) return float.MaxValue;
        var a0 = V(a.Mesh, a.Mesh.m_edges[ea].m_a) + a.Offset; var a1 = V(a.Mesh, a.Mesh.m_edges[ea].m_b) + a.Offset;
        var b0 = V(b.Mesh, b.Mesh.m_edges[eb].m_a) + b.Offset; var b1 = V(b.Mesh, b.Mesh.m_edges[eb].m_b) + b.Offset;
        float best = float.MaxValue;
        for (int i = 0; i <= 16; i++)
            for (int j = 0; j <= 16; j++)
                best = MathF.Min(best, Vector3.Distance(Vector3.Lerp(a0, a1, i / 16f), Vector3.Lerp(b0, b1, j / 16f)));
        return best;
    }
}
