using Andre.IO.VFS;
using Havok.hk2018;
using SoulsFormats;
using System.Collections.Concurrent;
using System.Numerics;
using System.Text.RegularExpressions;

// mapnav [regex] [max] : per map, the NVA against its navmesh HKX files (nvmhktbnd), across maps by NameID and map UID
static class MapNav
{
    static readonly ConcurrentDictionary<string, ConcurrentDictionary<string, int>> agg = new();
    static void C(string key, object v)
    {
        var d = agg.GetOrAdd(key, _ => new());
        d.AddOrUpdate(v?.ToString() ?? "null", 1, (_, x) => x + 1);
    }

    public record NavInfo(string Map, NVA_ER Nva, Dictionary<string, hkaiNavMesh> Meshes, Dictionary<string, hkaiDirectedGraphExplicitCost> Graphs);

    public static NavInfo Load(VirtualFileSystem fs, string map, Havok.HavokProfile profile)
    {
        string dir = $"/map/{map.Substring(0, 3)}/{map}/{map}";
        var nvaFile = fs.ReadFile(dir + ".nva.dcx");
        var bndFile = fs.ReadFile(dir + ".nvmhktbnd.dcx");
        if (nvaFile == null || bndFile == null) return null;
        var nvaBytes = DCX.Decompress(nvaFile.Value.ToArray(), out DCX.Type _);
        var nva = NVA_ER.Read(nvaBytes);
        C("nva roundtrip", nva.Write().AsSpan().SequenceEqual(nvaBytes));
        var meshes = new Dictionary<string, hkaiNavMesh>();
        var graphs = new Dictionary<string, hkaiDirectedGraphExplicitCost>();
        var bnd = BND4.Read(DCX.Decompress(bndFile.Value.ToArray(), out DCX.Type _));
        foreach (var f in bnd.Files)
        {
            var name = Path.GetFileName(f.Name);
            C("bnd file name form", Regex.Replace(name, "[0-9]", "#"));
            var bytes = f.Bytes.ToArray();
            if (DCX.Is(bytes)) bytes = DCX.Decompress(bytes).ToArray();
            var doc = Havok.Files.HavokFile.Read(bytes, profile);
            var key = name.Split('.')[0];
            C($"hkx object types [{name[0]}]", string.Join(",", doc.Objects.Select(o => o.GetType().Name).Distinct().OrderBy(x => x)));
            var nm = doc.ObjectsOf<hkaiNavMesh>().FirstOrDefault();
            if (nm != null)
            {
                C($"hkx [{name[0]}] faces", Bucket(nm.m_faces.Count));
                C($"hkx [{name[0]}] edge flags", string.Join("|", nm.m_edges.Select(e => (int)e.m_flags).Distinct().OrderBy(x => x)));
                C($"hkx [{name[0]}] faceData hi byte", string.Join("|", nm.m_faceData.Select(v => (uint)v >> 24).Distinct().OrderBy(x => x)));
                C($"hkx [{name[0]}] user edges", nm.m_faces.Any(f => f.m_numUserEdges > 0));
            }
            var mesh = doc.ObjectsOf<hkaiNavMesh>().FirstOrDefault();
            var graph = doc.ObjectsOf<hkaiDirectedGraphExplicitCost>().FirstOrDefault();
            if (mesh != null) meshes[key] = mesh;
            if (graph != null) graphs[key] = graph;
        }
        return new NavInfo(map, nva, meshes, graphs);
    }

    public static void Run(VirtualFileSystem fs, string dict, string[] args)
    {
        var re = new Regex(args.Length > 1 ? args[1] : ".");
        int max = args.Length > 2 ? int.Parse(args[2]) : int.MaxValue;
        var maps = Regex.Matches(dict, @"""/map/m\d\d/(m\d\d_\d\d_\d\d_\d\d)/\1\.nva\.dcx""").Select(m => m.Groups[1].Value)
            .Distinct().Where(m => re.IsMatch(m)).Take(max).ToList();
        Console.WriteLine($"{maps.Count} maps");
        var profile = Havok.HavokProfile.FromGame("ER");
        var infos = new ConcurrentDictionary<string, NavInfo>();
        Parallel.ForEach(maps, new ParallelOptions { MaxDegreeOfParallelism = 8 }, map =>
        {
            try { var i = Load(fs, map, profile); if (i != null) infos[map] = i; else C("missing", map); }
            catch (Exception e) { C("load error", $"{map}: {e.GetType().Name} {e.Message}"); }
        });

        // NameID -> (map, navmesh), for checks across maps
        var nameToNav = new Dictionary<int, (string Map, NVA_ER.Navmesh Nav)>();
        foreach (var i in infos.Values)
        {
            foreach (var e in i.Nva.Entries11) if (i.Nva.Entries11.Count != 1) C("e11 count", i.Nva.Entries11.Count);
            foreach (var n in i.Nva.Navmeshes) if (!nameToNav.TryAdd(n.NameID, (i.Map, n))) C("NameID repeats across maps", true);
        }
        var uids = infos.Values.GroupBy(i => i.Nva.Entries11.FirstOrDefault()?.Unk00 ?? 0).ToList();
        C("map uid shared by >1 map", uids.Count(g => g.Count() > 1));
        foreach (var g in uids.Where(g => g.Count() > 1).Take(5)) C("shared uid maps", $"{g.Key:X8}: {string.Join(" ", g.Select(x => x.Map).Take(4))}");

        foreach (var i in infos.Values)
        {
            var nva = i.Nva;
            C("count navmeshes", Bucket(nva.Navmeshes.Count));
            C("count facedata", Bucket(nva.FaceDatas.Count));
            C("count nodebanks", Bucket(nva.NodeBanks.Count));
            C("count entries3", Bucket(nva.Entries3.Count));
            C("count connectors", Bucket(nva.Connectors.Count));
            C("count levelconnectors", Bucket(nva.LevelConnectors.Count));
            C("count e9", Bucket(nva.Entries9.Count));
            C("count e10", Bucket(nva.Entries10.Count));
            C("count e13", Bucket(nva.Entries13?.Count ?? -1));
            C("n files == nva navmeshes", i.Meshes.Keys.Count(k => k[0] == 'n') == nva.Navmeshes.Count);
            C("o files == nva navmeshes", i.Meshes.Keys.Count(k => k[0] == 'o') == nva.Navmeshes.Count);
            foreach (var q in i.Meshes.Keys.Where(k => k[0] == 'q'))
            {
                C("q suffix", q.Substring(q.LastIndexOf('_')));
                C("q base is an NVA navmesh", nva.Navmeshes.Any(n => q.Contains($"_{n.ModelID:D6}_")));
            }
            C("hkx files", Bucket(i.Meshes.Count));
            C("hkx graphs", i.Graphs.Count == i.Meshes.Count ? "one per mesh" : $"{i.Graphs.Count} of {i.Meshes.Count}");

            int fdTotal = 0;
            foreach (var n in nva.Navmeshes)
            {
                var nkey = $"n{i.Map.Substring(1)}_{n.ModelID:D6}";
                var okey = $"o{i.Map.Substring(1)}_{n.ModelID:D6}";
                var key = i.Meshes.ContainsKey(nkey) ? nkey : null;
                C("navmesh has o file", i.Meshes.ContainsKey(okey));
                {
                    bool nm = i.Meshes.TryGetValue(nkey, out var nmesh) && nmesh.m_faces.Count == n.FaceCount;
                    bool om2 = i.Meshes.TryGetValue(okey, out var omesh) && omesh.m_faces.Count == n.FaceCount;
                    C("NVA FaceCount matches", nm && om2 ? "both" : nm ? "n only" : om2 ? "o only" : "neither");
                    // NameID as a decimal: P AA BB CC III, like 3 33 41 00 000 for m60_33_41_00
                    uint id = (uint)n.NameID;
                    string all = i.Map.Substring(1).Replace("_", ""); // AABBCCDD
                    uint p = id / 1000000000, mid = id / 1000 % 1000000, ix = id % 1000;
                    string form = mid.ToString("D6") == all.Substring(0, 6) ? "AABBCC" : mid.ToString("D6") == all.Substring(2, 6) ? "BBCCDD" : "other";
                    C("NameID middle digits", $"{i.Map.Substring(0, 3)} {form} prefix {p}");
                    int pos = nva.Navmeshes.IndexOf(n), half = nva.Navmeshes.Count / 2;
                    C("NameID prefix >= 2 iff second half", (p >= 2) == (pos >= half) && nva.Navmeshes.Count % 2 == 0);
                    C("NameID index == position within half", ix == (pos >= half ? pos - half : pos));
                    if (pos >= half && nva.Navmeshes.Count % 2 == 0)
                    {
                        var first = nva.Navmeshes[pos - half];
                        C("second half mirrors first (ModelID, NameID-2e9)", first.ModelID == n.ModelID && (uint)first.NameID + 2000000000u == id);
                    }
                    C("NameID odd billions (game's 140BDA2A0 test)", (id / 1000000000) & 1);
                }
                if (key != null && i.Meshes.TryGetValue(okey, out var om))
                {
                    var nm2 = i.Meshes[key];
                    C("o vs n faces", om.m_faces.Count == nm2.m_faces.Count ? "eq" : om.m_faces.Count < nm2.m_faces.Count ? "o fewer" : "o more");
                    C("o vs n verts", om.m_vertices.Count == nm2.m_vertices.Count ? "eq" : om.m_vertices.Count < nm2.m_vertices.Count ? "o fewer" : "o more");
                    C("o vs n aabb equal", om.m_aabb.m_min == nm2.m_aabb.m_min && om.m_aabb.m_max == nm2.m_aabb.m_max);
                    C("o FaceCount == NVA", om.m_faces.Count == n.FaceCount);
                }
                C("navmesh has hkx (by ModelID)", key != null);
                C("nav Unk3C", n.Unk3C);
                C("nav Unk44", n.Unk44);
                C("nav Unk4C", n.Unk4C);
                C("nav Unk50", $"{n.Unk50:X}");
                C("nav Unk54..5C", $"{n.Unk54},{n.Unk58},{n.Unk5C}");
                C("nav W", $"{n.PositionW},{n.RotationW},{n.ScaleW}");
                C("nav scale one", n.Scale == Vector3.One);
                C("nav rot zero", n.Rotation == Vector3.Zero);
                C("nav pos zero", n.Position == Vector3.Zero);
                C("nav gate nodes", Bucket(n.GateNodes.Count));
                C("nav v4 block nonzero", n.ConnectedNavmeshes.Any(x => x != 0));
                C("nav FaceDataIndex == running total", n.FaceDataIndex == fdTotal);
                fdTotal += n.FaceCount;
                if (key == null) continue;
                var m = i.Meshes[key];
                C("FaceCount == hkx faces", n.FaceCount == m.m_faces.Count);
                C("hkx faceDataStriding", m.m_faceDataStriding);
                C("hkx edgeDataStriding", m.m_edgeDataStriding);
                C("hkx userData", m.m_userData == 0 ? "0" : m.m_userData == (ulong)(uint)n.NameID ? "NameID" : "other");
                C("hkx flags", m.m_flags);
                C("hkx streamingSets", Bucket(m.m_streamingSets.Count));
                C("hkx erosionRadius", m.m_erosionRadius);
                C("hkx clearance seeding", m.m_clearanceCacheSeedingDataSet != null);
                foreach (var fl in m.m_edges.Select(e => (int)e.m_flags).Distinct()) C("hkx edge flag values", fl);
                C("hkx user edges", m.m_faces.Any(f => f.m_numUserEdges > 0));
                C("hkx clusters", m.m_faces.Any(f => f.m_clusterIndex >= 0) ? "set" : "none");
                if (m.m_faceDataStriding > 0)
                {
                    var fdv = m.m_faceData.Distinct().ToList();
                    C("hkx faceData distinct", Bucket(fdv.Count));
                    foreach (var v in fdv.Take(50)) C("hkx faceData value bits", $"{(uint)v >> 24:X2}xxxxxx");
                }
                if (m.m_edgeDataStriding > 0) foreach (var v in m.m_edgeData.Distinct().Take(50)) C("hkx edgeData values", Bucket(v));
                // NVA FaceData vs hkx faceData
                if (m.m_faceDataStriding == 1 && n.FaceDataIndex + n.FaceCount <= nva.FaceDatas.Count)
                {
                    int eq0 = 0, eq4 = 0;
                    for (int f = 0; f < n.FaceCount; f++) { if (nva.FaceDatas[n.FaceDataIndex + f].Unk00 == m.m_faceData[f]) eq0++; if (nva.FaceDatas[n.FaceDataIndex + f].Unk04 == m.m_faceData[f]) eq4++; }
                    C("NVA FaceData.Unk00 == hkx faceData", eq0 == n.FaceCount ? "all" : eq0 == 0 ? "none" : "some");
                    C("NVA FaceData.Unk04 == hkx faceData", eq4 == n.FaceCount ? "all" : eq4 == 0 ? "none" : "some");
                }
                foreach (var ss in m.m_streamingSets)
                {
                    var s = ss.m_streamingSet;
                    if (s == null) { C("ss null", true); continue; }
                    bool sideA = ss.m_side == hkaiAnnotatedStreamingSet.Side.SIDE_A;
                    uint mine = sideA ? s.m_aSectionUid : s.m_bSectionUid;
                    uint other = sideA ? s.m_bSectionUid : s.m_aSectionUid;
                    C("ss side", ss.m_side);
                    C("ss my uid == NameID", (int)mine == n.NameID);
                    var target = nameToNav.TryGetValue((int)other, out var t) ? (t.Map == i.Map ? "same map" : "other map") : "unknown";
                    C("ss other uid resolves to", target);
                    C("ss graph,volume conns", $"{Bucket(s.m_graphConnections.Count)},{s.m_volumeConnections.Count}");
                    C("ss aabbs per conn", $"{s.m_aConnectionAabbs.Count == s.m_meshConnections.Count},{s.m_bConnectionAabbs.Count == s.m_meshConnections.Count}");
                    if (target == "same map" && sideA)
                    {
                        var con = nva.Connectors.FirstOrDefault(c => c.MainNameID == (int)s.m_aSectionUid && c.TargetNameID == (int)s.m_bSectionUid);
                        var rev = nva.Connectors.FirstOrDefault(c => c.MainNameID == (int)s.m_bSectionUid && c.TargetNameID == (int)s.m_aSectionUid);
                        C("same-map ss (A side) has NVA connector", con != null ? "A->B" : rev != null ? "B->A only" : "none");
                        con ??= rev;
                        if (con != null)
                        {
                            C("NVA connector conns == ss mesh conns", con.NavMeshConnections.Count == s.m_meshConnections.Count);
                            C("NVA connector graph conns == ss graph conns", con.GraphConnections.Count == s.m_graphConnections.Count);
                            if (con.NavMeshConnections.Count == s.m_meshConnections.Count)
                            {
                                bool fwd = con.NavMeshConnections.Zip(s.m_meshConnections).All(z => z.First.FaceIndex == z.Second.m_aFaceEdgeIndex.m_faceIndex && z.First.EdgeIndex == z.Second.m_aFaceEdgeIndex.m_edgeIndex && z.First.OppositeFaceIndex == z.Second.m_bFaceEdgeIndex.m_faceIndex && z.First.OppositeEdgeIndex == z.Second.m_bFaceEdgeIndex.m_edgeIndex);
                                C("NVA connector conns identical to ss", fwd);
                            }
                        }
                    }
                }
                if (i.Graphs.TryGetValue(key, out var g))
                {
                    C("graph nodes vs gate nodes", g.m_positions.Count == n.GateNodes.Count ? "eq" : $"{Bucket(g.m_positions.Count)} vs {Bucket(n.GateNodes.Count)}");
                    C("graph streamingSets", Bucket(g.m_streamingSets.Count));
                    C("graph node/edge data striding", $"{g.m_nodeDataStriding},{g.m_edgeDataStriding}");
                }
            }
            C("NVA FaceDatas == sum FaceCount", nva.FaceDatas.Count == fdTotal);
            foreach (var fd in nva.FaceDatas.Take(2000)) { C("facedata Unk00", Bucket(fd.Unk00)); C("facedata Unk04", Bucket(fd.Unk04)); }
            foreach (var nb in nva.NodeBanks) { C("nodebank faces", Bucket(nb.Faces.Count)); C("nodebank EntityID set", nb.EntityID > 0); C("nodebank Unk0C", nb.Unk0C); C("nodebank BankID", Bucket(nb.BankID)); }
            foreach (var c in nva.Connectors)
            {
                C("connector main in map", nva.Navmeshes.Any(n => n.NameID == c.MainNameID));
                C("connector target", nva.Navmeshes.Any(n => n.NameID == c.TargetNameID) ? "same map" : nameToNav.ContainsKey(c.TargetNameID) ? "other map" : "unknown");
                C("connector graph conns", Bucket(c.GraphConnections.Count));
                C("connector Unk14,1C", $"{c.Unk14},{c.Unk1C}");
            }
            foreach (var lc in nva.LevelConnectors)
            {
                C("levelconn NavmeshID", nva.Navmeshes.Any(n => n.NameID == lc.NavmeshID) ? "NameID here" : nameToNav.ContainsKey(lc.NavmeshID) ? "NameID other map" : "other");
                C("levelconn Unk14,18,1C", $"{lc.Unk14},{lc.Unk18},{lc.Unk1C}");
                C("levelconn W", lc.Position.W);
            }
            foreach (var e in nva.Entries3) C("e3 vectors W", string.Join(",", e.Vectors.Select(v => v.W)));
            foreach (var e in nva.Entries9) C("e9", $"{Bucket(e.Unk00)},{Bucket(e.Unk04)},{Bucket(e.Unk08)},{e.Unk0C}");
            foreach (var e in nva.Entries10) C("e10 ints", $"{Bucket(e.Unk10)},{e.Unk14},{e.Unk18},{e.Unk1C}");
            foreach (var e in nva.Entries11) C("e11 rest", $"{e.Unk04},{e.Unk08},{e.Unk0C}");
            foreach (var e in nva.Entries13 ?? new()) C("e13 ints", string.Join(",", e.Ints.Select(Bucket)));
        }

        foreach (var (k, d) in agg.OrderBy(x => x.Key))
            Console.WriteLine($"{k} [{d.Count}]: {string.Join(", ", d.OrderByDescending(x => x.Value).Take(10).Select(x => $"{x.Key} x{x.Value}"))}");
    }

    static string Bucket(int v) => v switch { < 0 => v.ToString(), 0 => "0", 1 => "1", < 5 => "2-4", < 17 => "5-16", < 100 => "17-99", < 1000 => "100-999", < 100000 => "1k-99k", _ => "big" };
}

static class MapNavRows
{
    // navrows <map> : every NVA navmesh with its NameID digits and the files its ModelID names
    public static void Run(Andre.IO.VFS.VirtualFileSystem fs, string map)
    {
        var info = MapNav.Load(fs, map, Havok.HavokProfile.FromGame("ER"));
        var keys = info.Meshes.Keys.ToList();
        Console.WriteLine($"{map}: {info.Nva.Navmeshes.Count} navmeshes, files {keys.Count} ({keys.Count(k => k[0]=='n')} n, {keys.Count(k => k[0]=='o')} o, {keys.Count(k => k[0]=='q')} q)");
        foreach (var k in keys.Where(k => k[0] == 'q').Take(8)) Console.WriteLine($"  q file {k}: faces {info.Meshes[k].m_faces.Count}");
        int idx = 0;
        foreach (var n in info.Nva.Navmeshes)
        {
            string m = map.Substring(1);
            var files = keys.Where(k => k.EndsWith($"_{n.ModelID:D6}") || k.Contains($"_{n.ModelID:D6}_")).Select(k => $"{k[0]}{(k[0]=='q' ? k.Substring(k.Length - 2) : "")}:{info.Meshes[k].m_faces.Count}");
            Console.WriteLine($"  [{idx++,3}] NameID {(uint)n.NameID,10} Model {n.ModelID,6} Faces {n.FaceCount,5} Gates {n.GateNodes.Count,2} Unk4C {n.Unk4C} Unk50 {n.Unk50:X} rot {n.Rotation}  files {string.Join(" ", files)}");
        }
    }
}

static class MapNavValues
{
    // navvalues <regex> <max> : the face and edge data values of the n and o files
    public static void Run(Andre.IO.VFS.VirtualFileSystem fs, string dict, string re, int max)
    {
        var maps = Regex.Matches(dict, @"""/map/m\d\d/(m\d\d_\d\d_\d\d_\d\d)/\1\.nva\.dcx""").Select(m => m.Groups[1].Value).Distinct().Where(m => Regex.IsMatch(m, re)).Take(max).ToList();
        var face = new Dictionary<string, Dictionary<uint, long>> { ["n"] = new(), ["o"] = new() };
        var edge = new Dictionary<string, Dictionary<uint, long>> { ["n"] = new(), ["o"] = new() };
        var edgeByFlag = new Dictionary<string, long>();
        foreach (var map in maps)
        {
            var info = MapNav.Load(fs, map, Havok.HavokProfile.FromGame("ER"));
            if (info == null) continue;
            foreach (var (k, m) in info.Meshes)
            {
                var t = k.Substring(0, 1); if (!face.ContainsKey(t)) continue;
                foreach (var v in m.m_faceData) face[t][(uint)v] = face[t].GetValueOrDefault((uint)v) + 1;
                if (m.m_edgeDataStriding == 1)
                    for (int e = 0; e < m.m_edges.Count; e++)
                    {
                        uint v = (uint)m.m_edgeData[e];
                        edge[t][v] = edge[t].GetValueOrDefault(v) + 1;
                        var key = $"{t} flags {(int)m.m_edges[e].m_flags} data {(v == 0 ? "0" : "nonzero")} cost {(m.m_edges[e].m_userEdgeCost == 0 ? "0" : "set")} opp {(m.m_edges[e].m_oppositeFace == 0xFFFFFFFF ? "none" : "face")}";
                        edgeByFlag[key] = edgeByFlag.GetValueOrDefault(key) + 1;
                    }
            }
        }
        foreach (var t in new[] { "n", "o" })
        {
            Console.WriteLine($"{t} faceData: {string.Join(", ", face[t].OrderByDescending(x => x.Value).Take(16).Select(x => $"{x.Key:X8} x{x.Value}"))}");
            Console.WriteLine($"{t} edgeData: {string.Join(", ", edge[t].OrderByDescending(x => x.Value).Take(16).Select(x => $"{x.Key:X8} x{x.Value}"))}");
        }
        foreach (var (k, v) in edgeByFlag.OrderBy(x => x.Key)) Console.WriteLine($"{k}: {v}");
    }
}
