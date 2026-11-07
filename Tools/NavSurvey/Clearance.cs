using Andre.IO.VFS;
using Havok.hk2018;
using SoulsFormats;
using StudioCore.Editors.MapEditor;
using System.Numerics;
using System.Text.RegularExpressions;

// clearance dump <map> <file regex> : the clearance caches of a navmesh file next to its geometry, to work out the values.
// clearance why <map> <file> <vertex> : the edges near a vertex, with their flags and stored values.
// clearance fit <map regex> <max> <variant> : rules for the values (xz/3d, flood, refine, round, pairs, lattice, show)
//   against the game's first initial cache.
// clearance recalc <map regex> <max> : NavClearance.Recalculate (the Edit tab's commit) on a 3 m region around a random
//   vertex of each file, with nothing moved, against the game's values; and with the region moved, that the layout holds.
static class Clearance
{
    public static void Run(VirtualFileSystem fs, string dict, string[] args)
    {
        if (args[1] == "dump") { Dump(fs, args[2], new Regex(args[3])); return; }
        if (args[1] == "recalc") { Recalc(fs, dict, new Regex(args[2]), int.Parse(args[3])); return; }
        if (args[1] == "why") { Why(fs, args[2], args[3], int.Parse(args[4])); return; }
        if (args[1] == "fit") { Fit(fs, dict, new Regex(args[2]), int.Parse(args[3]), args.Length > 4 ? args[4] : "xz"); return; }
    }

    static void Recalc(VirtualFileSystem fs, string dict, Regex re, int max)
    {
        var maps = Regex.Matches(dict, @"""/map/m\d\d/(m\d\d_\d\d_\d\d_\d\d)/\1\.nva\.dcx""").Select(m => m.Groups[1].Value)
            .Distinct().Where(m => re.IsMatch(m)).Take(max).ToList();
        var c = new Dictionary<string, long>();
        var examples = new Dictionary<string, string>();
        void Count(string key, long n = 1, string example = null) { c[key] = c.GetValueOrDefault(key) + n; if (example != null) examples.TryAdd(key, example); }
        string Diff(int d) => d == 0 ? "exact" : Math.Abs(d) == 1 ? "±1" : Math.Abs(d) <= 3 ? "±2..3" : Math.Abs(d) <= 10 ? "±4..10" : d > 0 ? ">+10 (ours higher)" : "<-10 (ours lower)";
        var rng = new Random(11);
        var watch = System.Diagnostics.Stopwatch.StartNew();
        long recalcTicks = 0;

        foreach (var mapId in maps)
        {
            foreach (var (name, doc) in Files(fs, mapId))
            {
                var mesh = doc.ObjectsOf<hkaiNavMesh>().First();
                if (mesh.m_faces.Count == 0 || mesh.m_clearanceCacheSeedingDataSet == null) continue;
                Count("files");

                var positions = Enumerable.Range(0, mesh.m_vertices.Count).Select(i => V(mesh, i)).ToList();
                var centre = positions[rng.Next(positions.Count)];
                var region = Enumerable.Range(0, positions.Count).Where(i => Vector2.Distance(new(positions[i].X, positions[i].Z), new(centre.X, centre.Z)) <= 3f).ToHashSet();

                // Nothing moved: the recomputed values against the game's
                var copy = NavModelEdit.Clone(doc);
                var copyMesh = copy.ObjectsOf<hkaiNavMesh>().First();
                var t0 = System.Diagnostics.Stopwatch.GetTimestamp();
                var result = NavClearance.Recalculate(copyMesh, positions, region);
                recalcTicks += System.Diagnostics.Stopwatch.GetTimestamp() - t0;
                Count("caches recalculated", result.Caches);
                Count("caches skipped", result.Skipped);

                var game = mesh.m_clearanceCacheSeedingDataSet.m_cacheDatas[0].m_initialCache;
                var ours = copyMesh.m_clearanceCacheSeedingDataSet.m_cacheDatas[0].m_initialCache;
                for (int v = 0; v < game.m_vertexClearances.Count; v++)
                    if (game.m_vertexClearances[v] != ours.m_vertexClearances[v] || region.Contains(v))
                        Count($"vertex (region or changed): {Diff(ours.m_vertexClearances[v] - game.m_vertexClearances[v])}", 1, game.m_vertexClearances[v] == ours.m_vertexClearances[v] ? null : $"{name} v{v}");
                for (int k = 0; k < game.m_mcpData.Count; k++)
                {
                    var g = game.m_mcpData[k]; var o = ours.m_mcpData[k];
                    if (g.m_clearance == o.m_clearance && g.m_interpolant == o.m_interpolant) continue;
                    Count($"edge changed: clearance {Diff(o.m_clearance - g.m_clearance)}", 1, $"{name} e{k} {g.m_clearance}->{o.m_clearance}");
                    if (g.m_clearance == o.m_clearance) Count($"edge changed: interpolant only, {Diff(o.m_interpolant - g.m_interpolant)}");
                }
                for (int i = 0; i < game.m_edgePairClearances.Count; i++)
                    if (game.m_edgePairClearances[i] != ours.m_edgePairClearances[i])
                        Count($"pair changed: {Diff(ours.m_edgePairClearances[i] - game.m_edgePairClearances[i])}");
                Count("edge values recomputed and changed", result.Edges);

                // Moved: the region raised and shifted; the layout must hold and the values stay in range
                var moved = positions.Select((p, i) => region.Contains(i) ? p + new Vector3(0.4f, 0.3f, -0.25f) : p).ToList();
                var built = NavVertexEdit.Build(doc, moved, out var movedResult);
                var builtMesh = NavModelEdit.Clone(built).ObjectsOf<hkaiNavMesh>().First();
                var cache = builtMesh.m_clearanceCacheSeedingDataSet.m_cacheDatas[0].m_initialCache;
                bool layout = cache.m_vertexClearances.Count == game.m_vertexClearances.Count && cache.m_mcpData.Count == game.m_mcpData.Count
                    && cache.m_edgePairClearances.Count == game.m_edgePairClearances.Count && cache.m_faceOffsets.SequenceEqual(game.m_faceOffsets);
                bool range = cache.m_mcpData.All(m => m.m_clearance <= 254 && m.m_interpolant <= 253) && cache.m_vertexClearances.All(x => x <= 254 || x == 0xFF);
                bool ff = Enumerable.Range(0, mesh.m_faces.Count).All(f => (f + 1 < mesh.m_faces.Count ? cache.m_edgePairClearances[(int)cache.m_faceOffsets[f + 1] - 1] : cache.m_edgePairClearances[^1]) == 0xFF);
                Count(layout ? "moved: cache layout kept" : "moved: cache layout changed", 1, layout ? null : name);
                Count(range ? "moved: values in range" : "moved: values out of range", 1, range ? null : name);
                Count(ff ? "moved: every face block ends in FF" : "moved: a face block lost its FF", 1, ff ? null : name);
                Count("moved: vertex values changed", movedResult.Vertices);
                Count("moved: edge values changed", movedResult.Edges);
                Count("moved: pair values changed", movedResult.Pairs);
            }
        }

        foreach (var (k, v) in c.OrderBy(x => x.Key)) Console.WriteLine($"{v,12}  {k}{(examples.TryGetValue(k, out var ex) ? "   e.g. " + ex : "")}");
        Console.WriteLine($"recalculate (nothing moved) took {recalcTicks * 1000.0 / System.Diagnostics.Stopwatch.Frequency:0} ms in all; whole run {watch.ElapsedMilliseconds} ms");
    }

    /// <summary>The boundary segments of a mesh, bucketed in a grid on X/Z with cells the size of the ceiling.</summary>
    sealed class Boundary
    {
        public readonly List<(Vector3 A, Vector3 B)> Segments = new();
        readonly Dictionary<(int, int), List<int>> Grid = new();
        readonly float Cell;
        public readonly bool Planar;

        public Boundary(hkaiNavMesh mesh, float cell, bool planar)
        {
            Cell = cell;
            Planar = planar;
            foreach (var face in mesh.m_faces)
                for (int k = face.m_startEdgeIndex; k < face.m_startEdgeIndex + face.m_numEdges; k++)
                {
                    var e = mesh.m_edges[k];
                    if (e.m_oppositeFace != 0xFFFFFFFF) continue;
                    var a = V(mesh, e.m_a); var b = V(mesh, e.m_b);
                    int id = Segments.Count;
                    Segments.Add((a, b));
                    int x0 = (int)MathF.Floor(MathF.Min(a.X, b.X) / cell), x1 = (int)MathF.Floor(MathF.Max(a.X, b.X) / cell);
                    int z0 = (int)MathF.Floor(MathF.Min(a.Z, b.Z) / cell), z1 = (int)MathF.Floor(MathF.Max(a.Z, b.Z) / cell);
                    for (int x = x0; x <= x1; x++)
                        for (int z = z0; z <= z1; z++)
                            (Grid.TryGetValue((x, z), out var l) ? l : Grid[(x, z)] = new()).Add(id);
                }
        }

        /// <summary>The distance from a point to the nearest boundary segment, or the cap if none is nearer.</summary>
        public float Distance(Vector3 p, float cap)
        {
            float best = cap;
            int cx = (int)MathF.Floor(p.X / Cell), cz = (int)MathF.Floor(p.Z / Cell);
            for (int x = cx - 1; x <= cx + 1; x++)
                for (int z = cz - 1; z <= cz + 1; z++)
                {
                    if (!Grid.TryGetValue((x, z), out var list)) continue;
                    foreach (var id in list)
                    {
                        var (a, b) = Segments[id];
                        float d = SegmentDistance(p, a, b, Planar);
                        if (d < best) best = d;
                    }
                }
            return best;
        }
    }

    /// <summary>
    /// For each pair of a face's sides, the largest clearance at which a path inside the face joins them: a lattice of
    /// n steps over each fan triangle, swept in order of falling clearance with a union-find; a pair is joined when two
    /// groups holding one side each merge. Keyed by the sides' edge indices, lower first in the face's order.
    /// </summary>
    public static Dictionary<(int, int), int> WidestPairs(hkaiNavMesh mesh, int f, List<(Vector3, Vector3)> segs, Func<Vector3, int> clearance, int n)
    {
        var face = mesh.m_faces[f];
        int sides = face.m_numEdges;
        var c = new Vector3[sides];
        for (int m = 0; m < sides; m++) c[m] = V(mesh, mesh.m_edges[face.m_startEdgeIndex + m].m_a);

        var ids = new Dictionary<(int, int, int), int>();
        var pos = new List<Vector3>();
        var tags = new List<ulong>();
        int Id(int t, int a, int b)
        {
            if (t > 0 && b == 0) { t--; b = a; a = 0; }   // the diagonal shared with the triangle before
            if (!ids.TryGetValue((t, a, b), out var id))
            {
                id = pos.Count; ids[(t, a, b)] = id;
                pos.Add(c[0] + a / (float)n * (c[t + 1] - c[0]) + b / (float)n * (c[t + 2] - c[0]));
                ulong tag = 0;
                if (a + b == n) tag |= 1ul << (t + 1);            // side c[t+1] -> c[t+2]
                if (t == 0 && b == 0) tag |= 1ul;                  // side c0 -> c1
                if (t == sides - 3 && a == 0) tag |= 1ul << (sides - 1); // side c[n-1] -> c0
                tags.Add(tag);
            }
            return id;
        }

        var links = new List<(int, int)>();
        for (int t = 0; t + 2 < sides; t++)
            for (int a = 0; a <= n; a++)
                for (int b = 0; a + b <= n; b++)
                {
                    int me = Id(t, a, b);
                    if (a + 1 + b <= n) links.Add((me, Id(t, a + 1, b)));
                    if (a + b + 1 <= n) links.Add((me, Id(t, a, b + 1)));
                    if (a + 1 <= n && b >= 1) links.Add((me, Id(t, a + 1, b - 1)));
                }

        var value = pos.Select(clearance).ToArray();
        var neighbours = new List<int>[pos.Count];
        for (int i = 0; i < pos.Count; i++) neighbours[i] = new();
        foreach (var (x, y) in links) { neighbours[x].Add(y); neighbours[y].Add(x); }

        var parent = Enumerable.Range(0, pos.Count).ToArray();
        var mask = tags.ToArray();
        int Find(int x) { while (parent[x] != x) x = parent[x] = parent[parent[x]]; return x; }
        var active = new bool[pos.Count];
        var result = new Dictionary<(int, int), int>();

        void Join(ulong ma, ulong mb, int at)
        {
            for (int i = 0; i < sides; i++)
                if ((ma & (1ul << i)) != 0)
                    for (int j = 0; j < sides; j++)
                        if ((mb & (1ul << j)) != 0 && i != j)
                        {
                            var key = (face.m_startEdgeIndex + Math.Min(i, j), face.m_startEdgeIndex + Math.Max(i, j));
                            result.TryAdd(key, at);
                        }
        }

        foreach (var p in Enumerable.Range(0, pos.Count).OrderByDescending(i => value[i]))
        {
            active[p] = true;
            // A point on two sides (a corner) joins them by itself
            Join(mask[p], mask[p], value[p]);
            foreach (var q in neighbours[p])
            {
                if (!active[q]) continue;
                int rp = Find(p), rq = Find(q);
                if (rp == rq) continue;
                Join(mask[rp], mask[rq], value[p]);
                parent[rq] = rp;
                mask[rp] |= mask[rq];
            }
        }

        return result;
    }

    /// <summary>The distance between two segments: 0 if they cross (on X/Z when planar), else the nearest end to the other.</summary>
    public static float SegmentSegment(Vector3 a, Vector3 b, Vector3 c, Vector3 d, bool planar)
    {
        float Cross(Vector3 o, Vector3 p, Vector3 q) => (p.X - o.X) * (q.Z - o.Z) - (p.Z - o.Z) * (q.X - o.X);
        if (planar && Cross(a, b, c) * Cross(a, b, d) < 0 && Cross(c, d, a) * Cross(c, d, b) < 0)
            return 0;
        return MathF.Min(MathF.Min(SegmentDistance(a, c, d, planar), SegmentDistance(b, c, d, planar)),
            MathF.Min(SegmentDistance(c, a, b, planar), SegmentDistance(d, a, b, planar)));
    }

    public static float SegmentDistance(Vector3 p, Vector3 a, Vector3 b, bool planar)
    {
        if (planar) { p.Y = 0; a.Y = 0; b.Y = 0; }
        var ab = b - a;
        float len = ab.LengthSquared();
        float t = len < 1e-12f ? 0 : Math.Clamp(Vector3.Dot(p - a, ab) / len, 0, 1);
        return Vector3.Distance(p, a + ab * t);
    }

    /// <summary>
    /// Works the vertex clearances and the edges' maximum clearance points out from the geometry and compares them with
    /// the game's first initial cache: exact, within one step, and the spread of the rest.
    /// </summary>
    static void Fit(VirtualFileSystem fs, string dict, Regex re, int max, string variant)
    {
        var maps = Regex.Matches(dict, @"""/map/m\d\d/(m\d\d_\d\d_\d\d_\d\d)/\1\.nva\.dcx""").Select(m => m.Groups[1].Value)
            .Distinct().Where(m => re.IsMatch(m)).Take(max).ToList();
        bool planar = variant.Contains("xz");
        const int Samples = 254;
        var c = new Dictionary<string, long>();
        var examples = new Dictionary<string, string>();
        void Count(string key, long n = 1, string example = null) { c[key] = c.GetValueOrDefault(key) + n; if (example != null) examples.TryAdd(key, example); }
        string Diff(int d) => d == 0 ? "exact" : Math.Abs(d) == 1 ? "±1" : Math.Abs(d) <= 3 ? "±2..3" : Math.Abs(d) <= 10 ? "±4..10" : d > 0 ? ">+10 (ours higher)" : "<-10 (ours lower)";

        foreach (var mapId in maps)
        {
            foreach (var (name, doc) in Files(fs, mapId))
            {
                var mesh = doc.ObjectsOf<hkaiNavMesh>().First();
                var cache = mesh.m_clearanceCacheSeedingDataSet?.m_cacheDatas.FirstOrDefault()?.m_initialCache;
                if (mesh.m_faces.Count == 0 || cache == null) continue;
                Count("files");
                float ceiling = cache.m_clearanceCeiling, r2i = cache.m_clearanceRealToIntMultiplier;
                Count($"ceiling {ceiling} r2i {r2i}");
                int Q(float d) => variant.Contains("round") ? Math.Min(254, (int)MathF.Round(d * r2i)) : Math.Min(254, (int)(d * r2i));

                // The other initial caches, against the first
                foreach (var other in mesh.m_clearanceCacheSeedingDataSet.m_cacheDatas.Skip(1).Select(d => d.m_initialCache))
                {
                    bool same = other.m_vertexClearances.SequenceEqual(cache.m_vertexClearances) && other.m_edgePairClearances.SequenceEqual(cache.m_edgePairClearances)
                        && other.m_mcpData.Select(m => (m.m_clearance, m.m_interpolant)).SequenceEqual(cache.m_mcpData.Select(m => (m.m_clearance, m.m_interpolant)))
                        && other.m_clearanceCeiling == cache.m_clearanceCeiling;
                    Count(same ? "second cache: same as the first" : "second cache: differs", 1, same ? null : $"{name} ceiling {other.m_clearanceCeiling}");
                }
                Count($"caches per file: {mesh.m_clearanceCacheSeedingDataSet.m_cacheDatas.Count}");

                // Pairs: the smaller of the two linked edges' values, lower triangle by column, then FF
                for (int f = 0; f < mesh.m_faces.Count; f++)
                {
                    var face = mesh.m_faces[f];
                    var linked = new List<int>();
                    for (int k = face.m_startEdgeIndex; k < face.m_startEdgeIndex + face.m_numEdges; k++)
                        if (mesh.m_edges[k].m_oppositeFace != 0xFFFFFFFF) linked.Add(k);
                    for (int k = face.m_startUserEdgeIndex; k < face.m_startUserEdgeIndex + face.m_numUserEdges; k++)
                        if (mesh.m_edges[k].m_oppositeFace != 0xFFFFFFFF) linked.Add(k);
                    var expected = new List<byte>();
                    for (int j = 1; j < linked.Count; j++)
                        for (int i = 0; i < j; i++)
                            expected.Add(Math.Min(cache.m_mcpData[linked[i]].m_clearance, cache.m_mcpData[linked[j]].m_clearance));
                    expected.Add(0xFF);
                    int start = (int)cache.m_faceOffsets[f];
                    bool ok = start + expected.Count <= cache.m_edgePairClearances.Count && cache.m_edgePairClearances.Skip(start).Take(expected.Count).SequenceEqual(expected);
                    Count(ok ? "face pairs: min of the edges' values" : "face pairs: other", 1, ok ? null : $"{name} f{f}");
                    if (!ok && start + expected.Count <= cache.m_edgePairClearances.Count)
                        for (int i = 0; i < expected.Count - 1; i++)
                        {
                            int g = cache.m_edgePairClearances[start + i], m = expected[i];
                            Count(g == m ? "pair (in an 'other' face): = min" : g < m ? "pair (in an 'other' face): below min" : "pair (in an 'other' face): above min", 1, g == m ? null : $"{name} f{f} pair {i} game {g} min {m}");
                        }
                }

                var boundary = new Boundary(mesh, ceiling, planar);
                bool flood = variant.Contains("flood");

                // Pairs, worked out: the lowest clearance on the line between the two edges' maximum points (at the game's t)
                if (variant.Contains("pairs"))
                {
                    int shown = 0;
                    for (int f = 0; f < mesh.m_faces.Count; f++)
                    {
                        var face = mesh.m_faces[f];
                        var linked = new List<int>();
                        for (int k = face.m_startEdgeIndex; k < face.m_startEdgeIndex + face.m_numEdges; k++)
                            if (mesh.m_edges[k].m_oppositeFace != 0xFFFFFFFF) linked.Add(k);
                        int userStart = linked.Count;
                        for (int k = face.m_startUserEdgeIndex; k < face.m_startUserEdgeIndex + face.m_numUserEdges; k++)
                            if (mesh.m_edges[k].m_oppositeFace != 0xFFFFFFFF) linked.Add(k);
                        if (linked.Count < 2) continue;

                        var segs = new List<(Vector3, Vector3)>();
                        // every boundary within reach of the face: from its own edges
                        var reach = new HashSet<int> { f };
                        var queueF = new Queue<int>(reach);
                        var faceCorners = new List<Vector3>();
                        for (int k = face.m_startEdgeIndex; k < face.m_startEdgeIndex + face.m_numEdges; k++) faceCorners.Add(V(mesh, mesh.m_edges[k].m_a));
                        while (queueF.Count > 0)
                        {
                            var fc = mesh.m_faces[queueF.Dequeue()];
                            for (int k = fc.m_startEdgeIndex; k < fc.m_startEdgeIndex + fc.m_numEdges; k++)
                            {
                                var e = mesh.m_edges[k];
                                var a = V(mesh, e.m_a); var b = V(mesh, e.m_b);
                                if (e.m_oppositeFace == 0xFFFFFFFF) { segs.Add((a, b)); continue; }
                                int other = (int)(e.m_oppositeFace & 0x3FFFFF);
                                if (other >= mesh.m_faces.Count || reach.Contains(other)) continue;
                                float d = faceCorners.Min(q => SegmentDistance(q, a, b, true));
                                if (d > ceiling * 2) continue;
                                reach.Add(other); queueF.Enqueue(other);
                            }
                        }

                        Vector3 Mcp(int k)
                        {
                            var e = mesh.m_edges[k];
                            return Vector3.Lerp(V(mesh, e.m_a), V(mesh, e.m_b), cache.m_mcpData[k].m_interpolant / 253f);
                        }

                        var widest = !variant.Contains("lattice") ? new Dictionary<(int, int), int>() : WidestPairs(mesh, f, segs, p => Q(segs.Count == 0 ? ceiling : MathF.Min(ceiling, segs.Min(s => SegmentDistance(p, s.Item1, s.Item2, true)))), 16);
                        int start = (int)cache.m_faceOffsets[f], idx = 0;
                        for (int j = 1; j < linked.Count; j++)
                            for (int i = 0; i < j; i++, idx++)
                            {
                                int game = cache.m_edgePairClearances[start + idx];
                                int min = Math.Min(cache.m_mcpData[linked[i]].m_clearance, cache.m_mcpData[linked[j]].m_clearance);
                                bool user = i >= userStart || j >= userStart;
                                int ours = min;
                                if (!user)
                                    ours = Math.Min(min, widest.GetValueOrDefault((linked[i], linked[j]), min));
                                if (!user && game < min && face.m_numEdges == 3 && variant.Contains("show") && shown++ < 40)
                                {
                                    var ei = mesh.m_edges[linked[i]]; var ej = mesh.m_edges[linked[j]];
                                    int shared = ei.m_a == ej.m_b ? ei.m_a : ei.m_b == ej.m_a ? ei.m_b : -1;
                                    int third = Enumerable.Range(face.m_startEdgeIndex, 3).First(k => k != linked[i] && k != linked[j]);
                                    var et = mesh.m_edges[third];
                                    int far = et.m_a == shared ? et.m_b : et.m_a == ei.m_a || et.m_a == ei.m_b ? (et.m_b == shared ? et.m_a : et.m_b) : et.m_a;
                                    Console.WriteLine($"  {name} f{f}: game {game} mcp {cache.m_mcpData[linked[i]].m_clearance}@{cache.m_mcpData[linked[i]].m_interpolant} {cache.m_mcpData[linked[j]].m_clearance}@{cache.m_mcpData[linked[j]].m_interpolant} " +
                                        $"third {(et.m_oppositeFace == 0xFFFFFFFF ? "boundary" : cache.m_mcpData[third].m_clearance.ToString())} shared v{shared} vc {(shared >= 0 ? cache.m_vertexClearances[shared] : -1)} " +
                                        $"others vc {cache.m_vertexClearances[et.m_a]} {cache.m_vertexClearances[et.m_b]} lens {Vector3.Distance(V(mesh, ei.m_a), V(mesh, ei.m_b)):0.##} {Vector3.Distance(V(mesh, ej.m_a), V(mesh, ej.m_b)):0.##} {Vector3.Distance(V(mesh, et.m_a), V(mesh, et.m_b)):0.##}");
                                }
                                string kind = user ? "pair (user edge)" : game == min ? "pair (= min)" : "pair (below min)";
                                Count($"{kind}: line rule {Diff(ours - game)}", 1, ours == game ? null : $"{name} f{f} pair {idx} game {game} min {min} ours {ours}");
                            }
                    }
                }
                var faceOf = new int[mesh.m_edges.Count];
                var vertexFaces = new Dictionary<int, List<int>>();
                for (int f = 0; f < mesh.m_faces.Count; f++)
                {
                    var face = mesh.m_faces[f];
                    for (int k = face.m_startEdgeIndex; k < face.m_startEdgeIndex + face.m_numEdges; k++)
                    {
                        faceOf[k] = f;
                        int va = mesh.m_edges[k].m_a;
                        (vertexFaces.TryGetValue(va, out var l) ? l : vertexFaces[va] = new()).Add(f);
                    }
                }

                // The boundary segments reachable from start faces through faces whose shared edge lies within the ceiling of the query
                List<(Vector3, Vector3)> Reach(IEnumerable<int> start, Func<Vector3, Vector3, float> distanceTo)
                {
                    var found = new List<(Vector3, Vector3)>();
                    var seen = new HashSet<int>(start);
                    var queue = new Queue<int>(seen);
                    while (queue.Count > 0)
                    {
                        var face = mesh.m_faces[queue.Dequeue()];
                        for (int k = face.m_startEdgeIndex; k < face.m_startEdgeIndex + face.m_numEdges; k++)
                        {
                            var e = mesh.m_edges[k];
                            var a = V(mesh, e.m_a); var b = V(mesh, e.m_b);
                            if (e.m_oppositeFace == 0xFFFFFFFF) { found.Add((a, b)); continue; }
                            int other = (int)(e.m_oppositeFace & 0x3FFFFF);
                            if (other >= mesh.m_faces.Count || seen.Contains(other) || distanceTo(a, b) > ceiling) continue;
                            seen.Add(other);
                            queue.Enqueue(other);
                        }
                    }
                    return found;
                }

                float Nearest(List<(Vector3 A, Vector3 B)> segments, Vector3 p)
                {
                    float best = ceiling;
                    foreach (var (a, b) in segments) best = MathF.Min(best, SegmentDistance(p, a, b, planar));
                    return best;
                }

                // Vertices of the original edges (user edge vertices are 0xFF)
                var userVerts = new HashSet<int>();
                foreach (var face in mesh.m_faces)
                    for (int k = face.m_startUserEdgeIndex; k < face.m_startUserEdgeIndex + face.m_numUserEdges; k++)
                    { userVerts.Add(mesh.m_edges[k].m_a); userVerts.Add(mesh.m_edges[k].m_b); }

                for (int v = 0; v < mesh.m_vertices.Count; v++)
                {
                    int game = cache.m_vertexClearances[v];
                    if (userVerts.Contains(v)) { Count(game == 0xFF ? "vertex (user edge): FF" : "vertex (user edge): not FF"); continue; }
                    if (game == 0xFF) { Count("vertex: game FF"); continue; }
                    var p = V(mesh, v);
                    float dist = flood
                        ? Nearest(Reach(vertexFaces.GetValueOrDefault(v) ?? new List<int>(), (a, b) => SegmentDistance(p, a, b, planar)), p)
                        : boundary.Distance(p, ceiling);
                    int ours = Q(dist);
                    Count($"{name[0]} vertex: {Diff(ours - game)}", 1, ours == game ? null : $"{name} v{v} game {game} ours {ours}");
                }

                foreach (var face in mesh.m_faces)
                    for (int k = face.m_startEdgeIndex; k < face.m_startEdgeIndex + face.m_numEdges; k++)
                    {
                        var e = mesh.m_edges[k];
                        var mcp = cache.m_mcpData[k];
                        if (e.m_oppositeFace == 0xFFFFFFFF)
                        {
                            Count(mcp.m_clearance == 0 && mcp.m_interpolant == 0 ? "edge (boundary): 0 at 0" : "edge (boundary): other", 1, mcp.m_clearance == 0 ? null : $"{name} e{k} {mcp.m_clearance} t {mcp.m_interpolant}");
                            continue;
                        }

                        var a = V(mesh, e.m_a); var b = V(mesh, e.m_b);
                        float best = -1; int bestI = 0;
                        List<(Vector3, Vector3)> near = flood
                            ? Reach(new[] { faceOf[k], (int)(e.m_oppositeFace & 0x3FFFFF) }.Where(f => f < mesh.m_faces.Count), (x, y) => SegmentSegment(a, b, x, y, planar))
                            : null;
                        for (int i = 0; i < Samples; i++)
                        {
                            var q = Vector3.Lerp(a, b, i / (float)(Samples - 1));
                            float d = flood ? Nearest(near, q) : boundary.Distance(q, ceiling);
                            if (d > best + 1e-6f) { best = d; bestI = i; }
                        }
                        if (variant.Contains("refine") && best < ceiling)
                        {
                            // The maximum lies between the samples next to the best one: a ternary search there
                            float lo = Math.Max(0, bestI - 1) / (float)(Samples - 1), hi = Math.Min(Samples - 1, bestI + 1) / (float)(Samples - 1);
                            float F(float t) => flood ? Nearest(near, Vector3.Lerp(a, b, t)) : boundary.Distance(Vector3.Lerp(a, b, t), ceiling);
                            for (int it = 0; it < 40; it++)
                            {
                                float m1 = lo + (hi - lo) / 3, m2 = hi - (hi - lo) / 3;
                                if (F(m1) < F(m2)) lo = m1; else hi = m2;
                            }
                            float tm = (lo + hi) / 2, dm = F(tm);
                            if (dm > best) { best = dm; bestI = (int)MathF.Round(tm * (Samples - 1)); }
                        }
                        int ours = Q(best);
                        Count($"{name[0]} edge: {Diff(ours - mcp.m_clearance)}", 1, ours == mcp.m_clearance ? null : $"{name} e{k} game {mcp.m_clearance} t {mcp.m_interpolant} ours {ours} t {bestI}");
                        if (ours == mcp.m_clearance && mcp.m_clearance < 254)
                            Count($"edge t (uncapped, value matched): {Diff(bestI - mcp.m_interpolant)}", 1, bestI == mcp.m_interpolant ? null : $"{name} e{k} game t {mcp.m_interpolant} ours {bestI}");
                        if (mcp.m_clearance == 254)
                        {
                            Count($"edge t when capped: {(mcp.m_interpolant == 127 ? "127" : mcp.m_interpolant == 0 ? "0" : mcp.m_interpolant == 253 ? "253" : "other")}");

                            // Rule: the midpoint if it is at the ceiling, else the start, else the end, else the first sample that is
                            float At(float t) => flood ? Nearest(near, Vector3.Lerp(a, b, t)) : boundary.Distance(Vector3.Lerp(a, b, t), ceiling);
                            int rule = Q(At(0.5f)) >= 254 ? 127 : Q(At(0)) >= 254 ? 0 : Q(At(1)) >= 254 ? 253 : -1;
                            if (rule < 0)
                                for (int i = 0; i < Samples; i++) if (Q(At(i / (float)(Samples - 1))) >= 254) { rule = i; break; }
                            Count(rule == mcp.m_interpolant ? "edge t when capped: rule exact" : Math.Abs(rule - mcp.m_interpolant) <= 3 ? "edge t when capped: rule ±1..3" : "edge t when capped: rule other", 1,
                                rule == mcp.m_interpolant ? null : $"{name} e{k} game {mcp.m_interpolant} rule {rule}");
                        }
                    }
            }
        }

        foreach (var (k, v) in c.OrderBy(x => x.Key)) Console.WriteLine($"{v,12}  {k}{(examples.TryGetValue(k, out var ex) ? "   e.g. " + ex : "")}");
    }

    static IEnumerable<(string Name, Havok.HavokDocument Doc)> Files(VirtualFileSystem fs, string mapId)
    {
        var profile = Havok.HavokProfile.FromGame("ER");
        var bnd = fs.ReadFile(NavigationReader.NvmPath(mapId));
        if (bnd == null) yield break;
        foreach (var f in BND4.Read(bnd.Value.ToArray()).Files)
        {
            var bytes = f.Bytes.ToArray();
            if (DCX.Is(bytes)) bytes = DCX.Decompress(bytes).ToArray();
            yield return (Path.GetFileNameWithoutExtension(f.Name), Havok.Files.HavokFile.Read(bytes, profile));
        }
    }

    /// <summary>The edges at a vertex, and the boundary edges nearest it, with their flags and what the game stores.</summary>
    static void Why(VirtualFileSystem fs, string mapId, string fileName, int vertex)
    {
        var (name, doc) = Files(fs, mapId).First(f => f.Name == fileName);
        var mesh = doc.ObjectsOf<hkaiNavMesh>().First();
        var cache = mesh.m_clearanceCacheSeedingDataSet.m_cacheDatas[0].m_initialCache;
        var p = V(mesh, vertex);
        Console.WriteLine($"v{vertex} {p} game {cache.m_vertexClearances[vertex]}");
        for (int f = 0; f < mesh.m_faces.Count; f++)
        {
            var face = mesh.m_faces[f];
            for (int k = face.m_startEdgeIndex; k < face.m_startEdgeIndex + face.m_numEdges; k++)
            {
                var e = mesh.m_edges[k];
                float d = SegmentDistance(p, V(mesh, e.m_a), V(mesh, e.m_b), true);
                if (d > 0.3f) continue;
                var fd = mesh.m_faceDataStriding > 0 ? mesh.m_faceData[f * mesh.m_faceDataStriding] : 0;
                Console.WriteLine($"  f{f} ({face.m_numEdges} edges, data {fd:X}) e{k} v{e.m_a}-v{e.m_b} len {Vector3.Distance(V(mesh, e.m_a), V(mesh, e.m_b)):0.###} " +
                    $"{(e.m_oppositeFace == 0xFFFFFFFF ? "boundary" : $"-> f{e.m_oppositeFace & 0x3FFFFF} (sec {e.m_oppositeFace >> 22})")} oppEdge {e.m_oppositeEdge:X} flags {(int)e.m_flags} d {d:0.###} " +
                    $"mcp {cache.m_mcpData[k].m_clearance} t {cache.m_mcpData[k].m_interpolant} dy {V(mesh, e.m_a).Y - p.Y:0.##}");
            }
        }
    }

    public static Vector3 V(hkaiNavMesh m, int i) => new(m.m_vertices[i].X, m.m_vertices[i].Y, m.m_vertices[i].Z);

    static void Dump(VirtualFileSystem fs, string mapId, Regex re)
    {
        foreach (var (name, doc) in Files(fs, mapId))
        {
            if (!re.IsMatch(name)) continue;
            var mesh = doc.ObjectsOf<hkaiNavMesh>().First();
            if (mesh.m_faces.Count == 0) continue;
            Console.WriteLine($"== {name}: faces {mesh.m_faces.Count} edges {mesh.m_edges.Count} vertices {mesh.m_vertices.Count} erosion {mesh.m_erosionRadius}");
            var set = mesh.m_clearanceCacheSeedingDataSet;
            if (set == null) { Console.WriteLine("no seeding"); continue; }

            foreach (var data in set.m_cacheDatas)
            {
                var c = data.m_initialCache;
                Console.WriteLine($"cache id {data.m_id:X} info {data.m_info:X} mask {data.m_infoMask:X}: ceiling {c.m_clearanceCeiling} i2r {c.m_clearanceIntToRealMultiplier} r2i {c.m_clearanceRealToIntMultiplier} " +
                    $"offsets {c.m_faceOffsets.Count} pairs {c.m_edgePairClearances.Count} unused {c.m_unusedEdgePairElements} mcp {c.m_mcpData.Count} vtx {c.m_vertexClearances.Count} uncalc {c.m_uncalculatedFacesLowerBound}");
            }

            var cache = set.m_cacheDatas[0].m_initialCache;
            float s = cache.m_clearanceIntToRealMultiplier;

            Console.WriteLine("vertices: index pos clearance");
            for (int i = 0; i < mesh.m_vertices.Count; i++)
                Console.WriteLine($"  v{i} {V(mesh, i):0.###} {(cache.m_vertexClearances[i] == 0xFF ? "FF" : (cache.m_vertexClearances[i] * s).ToString("0.###"))} ({cache.m_vertexClearances[i]})");

            for (int f = 0; f < mesh.m_faces.Count; f++)
            {
                var face = mesh.m_faces[f];
                Console.WriteLine($"face {f}: offset {cache.m_faceOffsets[f]}");
                for (int k = face.m_startEdgeIndex; k < face.m_startEdgeIndex + face.m_numEdges; k++)
                {
                    var e = mesh.m_edges[k];
                    var mcp = cache.m_mcpData[k];
                    string opp = e.m_oppositeFace == 0xFFFFFFFF ? "boundary" : $"-> f{e.m_oppositeFace & 0x3FFFFF}";
                    Console.WriteLine($"  e{k} v{e.m_a}-v{e.m_b} len {Vector3.Distance(V(mesh, e.m_a), V(mesh, e.m_b)):0.###} {opp} flags {(int)e.m_flags} mcp clr {(mcp.m_clearance == 0xFF ? "FF" : (mcp.m_clearance * s).ToString("0.###"))} ({mcp.m_clearance}) t {mcp.m_interpolant}");
                }
                int start = (int)cache.m_faceOffsets[f];
                int end = f + 1 < mesh.m_faces.Count ? (int)cache.m_faceOffsets[f + 1] : cache.m_edgePairClearances.Count;
                Console.WriteLine($"  pairs: {string.Join(" ", cache.m_edgePairClearances.Skip(start).Take(Math.Max(0, end - start)).Select(b => b.ToString("X2")))}");
            }
        }
    }
}
