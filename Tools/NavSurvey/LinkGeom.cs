using Andre.IO.VFS;
using Havok.hk2018;
using SoulsFormats;
using System.Numerics;
using System.Text.RegularExpressions;

// linkgeom [regex] [max] : the shape of the game's user-edge links, for the generator's jump/fall heuristic. For each
// link (an unblocked user edge and the edge it pairs with): the height change and horizontal reach from its start to
// its end, by edge data type and by one-way or both ways; whether the start sits at a boundary edge of its face; the
// edge data's high byte against the drop; how far apart links between the same two faces are.
static class LinkGeom
{
    public static void Run(VirtualFileSystem fs, string dict, string[] args)
    {
        var re = new Regex(args.Length > 1 ? args[1] : ".");
        int max = args.Length > 2 ? int.Parse(args[2]) : 60;
        var maps = Regex.Matches(dict, @"""/map/m\d\d/(m\d\d_\d\d_\d\d_\d\d)/\1\.nva\.dcx""").Select(m => m.Groups[1].Value)
            .Distinct().Where(m => re.IsMatch(m)).Take(max).ToList();
        var profile = Havok.HavokProfile.FromGame("ER");

        // Per category: dy and dxz samples
        var dy = new Dictionary<string, List<float>>();
        var dxz = new Dictionary<string, List<float>>();
        var counts = new SortedDictionary<string, long>();
        var highByte = new SortedDictionary<int, List<float>>();
        var startToBoundary = new List<float>();
        var endToBoundary = new List<float>();
        var pairSpacing = new List<float>();
        var linksPerFace = new List<int>();
        void Count(string k) => counts[k] = counts.GetValueOrDefault(k) + 1;
        void Add(Dictionary<string, List<float>> d, string k, float v) { if (!d.TryGetValue(k, out var l)) d[k] = l = new(); l.Add(v); }

        foreach (var mapId in maps)
        {
            var bnd = fs.ReadFile(StudioCore.Editors.MapEditor.NavigationReader.NvmPath(mapId));
            if (bnd == null) continue;

            foreach (var f in BND4.Read(bnd.Value.ToArray()).Files)
            {
                var name = Path.GetFileNameWithoutExtension(f.Name);
                var set = name.StartsWith("o") ? "o" : "n";
                var bytes = f.Bytes.ToArray();
                if (DCX.Is(bytes)) bytes = DCX.Decompress(bytes).ToArray();
                var mesh = Havok.Files.HavokFile.Read(bytes, profile).ObjectsOf<hkaiNavMesh>().First();
                if (mesh.m_faces.Count == 0) continue;

                int orig = mesh.m_faces.Sum(x => x.m_numEdges);
                if (orig == mesh.m_edges.Count) continue;

                var owner = new int[mesh.m_edges.Count];
                for (int fi = 0; fi < mesh.m_faces.Count; fi++)
                {
                    var face = mesh.m_faces[fi];
                    for (int e = face.m_startUserEdgeIndex; face.m_numUserEdges > 0 && e < face.m_startUserEdgeIndex + face.m_numUserEdges; e++)
                        owner[e] = fi;
                    if (face.m_numUserEdges > 0) linksPerFace.Add(face.m_numUserEdges);
                }

                Vector3 V(int i) => new(mesh.m_vertices[i].X, mesh.m_vertices[i].Y, mesh.m_vertices[i].Z);
                Vector3 Mid(int e) => (V(mesh.m_edges[e].m_a) + V(mesh.m_edges[e].m_b)) / 2;

                // Distance in XZ from a point to the nearest boundary (unlinked original) edge of a face
                float ToBoundary(int face, Vector3 p)
                {
                    var fc = mesh.m_faces[face];
                    float best = float.MaxValue;
                    for (int e = fc.m_startEdgeIndex; e < fc.m_startEdgeIndex + fc.m_numEdges; e++)
                    {
                        if (mesh.m_edges[e].m_oppositeFace != 0xFFFFFFFF) continue;
                        var a = new Vector2(V(mesh.m_edges[e].m_a).X, V(mesh.m_edges[e].m_a).Z);
                        var b = new Vector2(V(mesh.m_edges[e].m_b).X, V(mesh.m_edges[e].m_b).Z);
                        var q = new Vector2(p.X, p.Z);
                        var ab = b - a;
                        float t = ab.LengthSquared() > 1e-9f ? Math.Clamp(Vector2.Dot(q - a, ab) / ab.LengthSquared(), 0, 1) : 0;
                        best = MathF.Min(best, Vector2.Distance(q, a + ab * t));
                    }
                    return best;
                }

                var byPair = new Dictionary<(int, int), List<Vector3>>();

                // Faces bucketed by XZ (2 m), for the floor under a link's midpoint
                var grid = new Dictionary<(int, int), List<int>>();
                for (int fi = 0; fi < mesh.m_faces.Count; fi++)
                {
                    var fc = mesh.m_faces[fi];
                    float x0 = float.MaxValue, x1 = float.MinValue, z0 = float.MaxValue, z1 = float.MinValue;
                    for (int e = fc.m_startEdgeIndex; e < fc.m_startEdgeIndex + fc.m_numEdges; e++)
                    {
                        var p = V(mesh.m_edges[e].m_a);
                        x0 = MathF.Min(x0, p.X); x1 = MathF.Max(x1, p.X); z0 = MathF.Min(z0, p.Z); z1 = MathF.Max(z1, p.Z);
                    }
                    for (int x = (int)MathF.Floor(x0 / 2); x <= (int)MathF.Floor(x1 / 2); x++)
                        for (int z = (int)MathF.Floor(z0 / 2); z <= (int)MathF.Floor(z1 / 2); z++)
                        {
                            if (!grid.TryGetValue((x, z), out var l)) grid[(x, z)] = l = new();
                            l.Add(fi);
                        }
                }

                // The height of the highest face under a point (XZ inside the face, by fan triangles), at or below a ceiling
                float? FloorUnder(Vector3 p, float ceiling)
                {
                    if (!grid.TryGetValue(((int)MathF.Floor(p.X / 2), (int)MathF.Floor(p.Z / 2)), out var l)) return null;
                    float? best = null;
                    foreach (var fi in l)
                    {
                        var fc = mesh.m_faces[fi];
                        var a0 = V(mesh.m_edges[fc.m_startEdgeIndex].m_a);
                        for (int k = 1; k + 1 < fc.m_numEdges; k++)
                        {
                            var b0 = V(mesh.m_edges[fc.m_startEdgeIndex + k].m_a);
                            var c0 = V(mesh.m_edges[fc.m_startEdgeIndex + k + 1].m_a);
                            float d = (b0.Z - c0.Z) * (a0.X - c0.X) + (c0.X - b0.X) * (a0.Z - c0.Z);
                            if (MathF.Abs(d) < 1e-9f) continue;
                            float u = ((b0.Z - c0.Z) * (p.X - c0.X) + (c0.X - b0.X) * (p.Z - c0.Z)) / d;
                            float v = ((c0.Z - a0.Z) * (p.X - c0.X) + (a0.X - c0.X) * (p.Z - c0.Z)) / d;
                            if (u < 0 || v < 0 || u + v > 1) continue;
                            float y = u * a0.Y + v * b0.Y + (1 - u - v) * c0.Y;
                            if (y <= ceiling && (best == null || y > best)) best = y;
                        }
                    }
                    return best;
                }

                for (int e = orig; e < mesh.m_edges.Count; e++)
                {
                    var edge = mesh.m_edges[e];
                    if (edge.m_oppositeEdge == 0xFFFFFFFF || (edge.m_oppositeEdge >> 22) != 0) { Count("unpaired"); continue; }
                    int o = (int)edge.m_oppositeEdge;
                    if (o < orig || o >= mesh.m_edges.Count) continue;
                    bool blocked = (edge.m_flags & hkaiNavMesh.EdgeFlagBits.EDGE_BLOCKED) != 0;
                    bool otherBlocked = (mesh.m_edges[o].m_flags & hkaiNavMesh.EdgeFlagBits.EDGE_BLOCKED) != 0;

                    // Each link once: from its unblocked edge; a both-ways link from its lower edge
                    if (blocked) continue;
                    if (!otherBlocked && o < e) continue;

                    int data = mesh.m_edgeData.Count > e ? mesh.m_edgeData[e] : 0;
                    string kind = (data & 0xFF) switch { 0x02 => "jump", 0x12 => "jump+cost", 0x04 => "fall", 0x14 => "fall+cost", 0 => "none", var x => $"0x{x:X2}" };
                    string way = otherBlocked ? "one-way" : "both";
                    string key = $"{set} {kind} {way}";
                    Count(key);

                    var a = Mid(e);
                    var b = Mid(o);
                    float h = b.Y - a.Y;
                    float d = new Vector2(b.X - a.X, b.Z - a.Z).Length();
                    Add(dy, key, h);
                    Add(dxz, key, d);

                    // Floor under the midpoint, below the lower end: how far under it (none = a hole)
                    var mid = (a + b) / 2;
                    float lower = MathF.Min(a.Y, b.Y);
                    var floor = FloorUnder(mid, lower + 0.3f);
                    Count($"{key}: mid floor {(floor == null ? "none" : lower - floor < 0.5f ? "at lower end" : lower - floor < 3 ? "0.5-3 m below" : "3+ m below")}");

                    // Floor under the start, pushed 0.5 m further out past the start: is there ground just off the ledge?
                    var outward = new Vector3(b.X - a.X, 0, b.Z - a.Z);
                    if (outward.LengthSquared() > 1e-6f)
                    {
                        var off = a + Vector3.Normalize(outward) * 0.9f;
                        var under = FloorUnder(off, a.Y + 0.3f);
                        Count($"{key}: 0.9 m out {(under == null ? "nothing" : a.Y - under < 0.5f ? "same level" : MathF.Abs(under.Value - b.Y) < 0.5f ? "landing level" : "other")}");
                    }

                    if ((data & 0x4) != 0)
                    {
                        int hb = (data >> 8) & 0xFF;
                        if (!highByte.TryGetValue(hb, out var l)) highByte[hb] = l = new();
                        l.Add(h);
                    }

                    startToBoundary.Add(ToBoundary(owner[e], a));
                    endToBoundary.Add(ToBoundary(owner[o], b));

                    var pk = (owner[e], owner[o]);
                    if (!byPair.TryGetValue(pk, out var pl)) byPair[pk] = pl = new();
                    pl.Add(a);
                }

                foreach (var (_, starts) in byPair)
                {
                    Count(starts.Count == 1 ? "face pairs with 1 link" : "face pairs with 2+ links");
                    for (int i = 1; i < starts.Count; i++)
                        pairSpacing.Add(Vector3.Distance(starts[i], starts[i - 1]));
                }
            }
        }

        static string Q(List<float> v)
        {
            if (v.Count == 0) return "-";
            var s = v.OrderBy(x => x).ToList();
            float P(float q) => s[Math.Clamp((int)(q * (s.Count - 1)), 0, s.Count - 1)];
            return $"n={s.Count} min {s[0]:F2} p5 {P(0.05f):F2} p25 {P(0.25f):F2} med {P(0.5f):F2} p75 {P(0.75f):F2} p95 {P(0.95f):F2} max {s[^1]:F2}";
        }

        Console.WriteLine($"{maps.Count} maps");
        foreach (var (k, n) in counts) Console.WriteLine($"{k,-28} {n}");
        Console.WriteLine();
        foreach (var k in dy.Keys.OrderBy(k => k))
        {
            Console.WriteLine($"{k}");
            Console.WriteLine($"   dy  {Q(dy[k])}");
            Console.WriteLine($"   dxz {Q(dxz[k])}");
        }
        Console.WriteLine();
        Console.WriteLine("fall edge data high byte -> dy");
        foreach (var (hb, l) in highByte) Console.WriteLine($"   0x{hb:X2}: {Q(l)}");
        Console.WriteLine();
        Console.WriteLine($"start to face boundary {Q(startToBoundary)}");
        Console.WriteLine($"end to face boundary   {Q(endToBoundary)}");
        Console.WriteLine($"spacing of links between the same faces {Q(pairSpacing)}");
        Console.WriteLine($"user edges per face {Q(linksPerFace.Select(x => (float)x).ToList())}");
    }
}
