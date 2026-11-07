using Andre.IO.VFS;
using Havok.hk2018;
using SoulsFormats;
using StudioCore.Editors.MapEditor;
using System.Numerics;
using System.Text.RegularExpressions;

// stitchsurvey [regex] [max] : how the game's NVA connectors (in-map) and NVC connectors (cross-map) match the geometry
// of the edges they join, so the Navigation tool can recompute them from the meshes.
static class StitchSurvey
{
    public static void Run(VirtualFileSystem fs, string dict, string[] args)
    {
        var re = new Regex(args.Length > 1 ? args[1] : ".");
        int max = args.Length > 2 ? int.Parse(args[2]) : 50;
        var maps = Regex.Matches(dict, @"""/map/m\d\d/(m\d\d_\d\d_\d\d_\d\d)/\1\.nva\.dcx""").Select(m => m.Groups[1].Value)
            .Distinct().Where(m => re.IsMatch(m)).Take(max).ToList();
        var nvcNames = Regex.Matches(dict, @"""/map/nvc/(m\d\d_\d\d_\d\d_\d\d_m\d\d_\d\d_\d\d_\d\d)\.nvc\.dcx""").Select(m => m.Groups[1].Value).Distinct().ToList();
        var profile = Havok.HavokProfile.FromGame("ER");
        var c = new Dictionary<string, long>();
        var examples = new Dictionary<string, string>();
        void Count(string key, string example = null) { c[key] = c.GetValueOrDefault(key) + 1; if (example != null) examples.TryAdd(key, example); }
        var deltas = new Dictionary<string, Dictionary<string, int>>();

        foreach (var mapId in maps)
        {
            var map = NavigationReader.Read(fs, mapId, profile, NavigationReader.NvcNamesFor(mapId, nvcNames));
            if (map?.Nva == null) continue;

            foreach (var e in map.Nva.Navmeshes)
                if (e.Rotation != Vector3.Zero || e.Position != Vector3.Zero) Count("entries moved or rotated", $"{mapId} {e.NameID} {e.Position} {e.Rotation}");

            // In-map connectors
            var seen = new HashSet<(int, int, int, int)>();
            foreach (var connector in map.Nva.Connectors)
            {
                var a = map.FindNavmesh(connector.MainNameID);
                var b = map.FindNavmesh(connector.TargetNameID);
                if (a == null || b == null) { Count("nva: connector names a missing navmesh"); continue; }
                Count($"nva: connector sets {NavigationMap.SetOf(a)}{NavigationMap.SetOf(b)}");
                Count($"nva: Unk14 {(connector.Unk14 == 0 ? "0" : "other")}, Unk1C {(connector.Unk1C == 0 ? "0" : "other")}", $"{mapId} {connector.Unk14} {connector.Unk1C}");
                if (connector.MainNameID == connector.TargetNameID) Count("nva: connector to itself", mapId);
                if (connector.MainNameID > connector.TargetNameID) Count("nva: connector main > target");
                else Count("nva: connector main < target");
                if (connector.NavMeshConnections.Count == 0) Count("nva: connector without connections", mapId);

                var ma = map.FileOf(a)?.Mesh; var mb = map.FileOf(b)?.Mesh;
                if (ma == null || mb == null) { Count("nva: connector to a mesh without a file"); continue; }

                foreach (var conn in connector.NavMeshConnections)
                {
                    Count("nva: connections");
                    if (!seen.Add((connector.MainNameID, conn.EdgeIndex, connector.TargetNameID, conn.OppositeEdgeIndex))) Count("nva: duplicate connection");
                    Classify("nva", ma, conn.FaceIndex, conn.EdgeIndex, a.Position, mb, conn.OppositeFaceIndex, conn.OppositeEdgeIndex, b.Position, Count, mapId);
                }
            }

            // Is each connection in both directions?
            foreach (var (ma, ea, mb, eb) in seen)
                Count(seen.Contains((mb, eb, ma, ea)) ? "nva: connection has its reverse" : "nva: connection without its reverse");

            // Cross-map: this map as A only, so each file is counted once
            foreach (var nvc in map.Nvcs.Where(n => n.MapA == mapId))
            {
                var other = NavigationReader.Read(fs, nvc.MapB, profile, Array.Empty<string>());
                if (other?.Nva == null) { Count("nvc: other map missing"); continue; }
                Count($"nvc: kinds {Kind(nvc.MapA)}-{Kind(nvc.MapB)}");
                var mirror = $"{nvc.MapB}_{nvc.MapA}";
                Count(nvcNames.Contains(mirror) ? "nvc: the mirrored file exists" : "nvc: no mirrored file", nvc.Name);
                if (nvc.Nvc.Connectors.Count == 0) Count("nvc: file without connectors");
                var offset = TileOffset(nvc.MapA, nvc.MapB);

                foreach (var connector in nvc.Nvc.Connectors)
                {
                    var a = map.FindNavmesh(connector.MainNameID);
                    var b = other.FindNavmesh(connector.TargetNameID);
                    if (a == null || b == null) { Count("nvc: connector names a missing navmesh"); continue; }
                    Count($"nvc: connector sets {NavigationMap.SetOf(a)}{NavigationMap.SetOf(b)}");
                    Count($"nvc: Unk14 {(connector.Unk14 == 0 ? "0" : "other")}, Unk1C {(connector.Unk1C == 0 ? "0" : "other")}", $"{nvc.Name} {connector.Unk14} {connector.Unk1C}");
                    var ma = map.FileOf(a)?.Mesh; var mb = other.FileOf(b)?.Mesh;
                    if (ma == null || mb == null) { Count("nvc: connector to a mesh without a file"); continue; }

                    foreach (var conn in connector.NavMeshConnections)
                    {
                        Count("nvc: connections");
                        if (conn.EdgeIndex >= ma.m_edges.Count || conn.OppositeEdgeIndex >= mb.m_edges.Count) { Count("nvc: edge out of range"); continue; }
                        var midA = Mid(ma, conn.EdgeIndex); var midB = Mid(mb, conn.OppositeEdgeIndex);
                        var delta = midA - midB;
                        var key = $"{Kind(nvc.MapA)}-{Kind(nvc.MapB)}";
                        if (!deltas.TryGetValue(key, out var d)) deltas[key] = d = new();
                        var rel = offset is Vector3 off ? $"vs tile formula {Round(delta - off)}" : $"delta {Round(delta)}";
                        d[rel] = d.GetValueOrDefault(rel) + 1;
                        if (offset is Vector3 o2)
                            Classify("nvc", ma, conn.FaceIndex, conn.EdgeIndex, Vector3.Zero, mb, conn.OppositeFaceIndex, conn.OppositeEdgeIndex, o2, Count, nvc.Name);
                    }
                }
            }
        }

        foreach (var (k, v) in c.OrderBy(x => x.Key)) Console.WriteLine($"{v,9}  {k}{(examples.TryGetValue(k, out var ex) ? "   e.g. " + ex : "")}");
        foreach (var (kind, d) in deltas)
        {
            Console.WriteLine($"nvc {kind} deltas (A minus B, rounded):");
            foreach (var (k, v) in d.OrderByDescending(x => x.Value).Take(8)) Console.WriteLine($"{v,9}    {k}");
        }
    }

    static string Round(Vector3 v) => $"({MathF.Round(v.X)}, {MathF.Round(v.Y)}, {MathF.Round(v.Z)})";

    public static string Kind(string mapId) => mapId.StartsWith("m60") || mapId.StartsWith("m61") ? $"{mapId[..3]}_{mapId[^1]}" : "legacy";

    /// <summary>Where map B's origin is in map A's space, for two open world tiles.</summary>
    public static Vector3? TileOffset(string a, string b)
    {
        static (int X, int Z, int Scale, string Area)? Tile(string m)
        {
            if (!(m.StartsWith("m60") || m.StartsWith("m61"))) return null;
            int x = int.Parse(m.Substring(4, 2)), z = int.Parse(m.Substring(7, 2)), s = int.Parse(m.Substring(11, 1));
            return (x, z, s, m[..3]);
        }
        var ta = Tile(a); var tb = Tile(b);
        if (ta == null || tb == null || ta.Value.Area != tb.Value.Area) return null;

        static Vector3 Origin((int X, int Z, int Scale, string Area) t)
        {
            int f = t.Scale == 1 ? 2 : t.Scale == 2 ? 4 : 1;
            var centre = t.Scale == 1 ? new Vector3(128, 0, 128) : t.Scale == 2 ? new Vector3(384, 0, 384) : Vector3.Zero;
            return new Vector3(t.X * f * 256, 0, t.Z * f * 256) + centre;
        }
        return Origin(tb.Value) - Origin(ta.Value);
    }

    static Vector3 V(hkaiNavMesh m, int i) => new(m.m_vertices[i].X, m.m_vertices[i].Y, m.m_vertices[i].Z);
    static Vector3 Mid(hkaiNavMesh m, int e) => (V(m, m.m_edges[e].m_a) + V(m, m.m_edges[e].m_b)) / 2;

    static float DistToLine(Vector3 p, Vector3 a, Vector3 b)
    {
        var ab = b - a; float t = ab.LengthSquared() < 1e-12f ? 0 : Vector3.Dot(p - a, ab) / ab.LengthSquared();
        return (a + ab * t - p).Length();
    }

    // How the two edges of a connection sit against each other
    static void Classify(string kind, hkaiNavMesh ma, int fa, int ea, Vector3 posA, hkaiNavMesh mb, int fb, int eb, Vector3 posB, Action<string, string> count, string where)
    {
        if (ea >= ma.m_edges.Count || eb >= mb.m_edges.Count) { count($"{kind}: edge out of range", where); return; }
        var A = ma.m_edges[ea]; var B = mb.m_edges[eb];
        bool inFaceA = fa < ma.m_faces.Count && ea >= ma.m_faces[fa].m_startEdgeIndex && ea < ma.m_faces[fa].m_startEdgeIndex + ma.m_faces[fa].m_numEdges;
        bool inFaceB = fb < mb.m_faces.Count && eb >= mb.m_faces[fb].m_startEdgeIndex && eb < mb.m_faces[fb].m_startEdgeIndex + mb.m_faces[fb].m_numEdges;
        count($"{kind}: edge in its face {(inFaceA && inFaceB ? "both" : "not both")}", inFaceA && inFaceB ? null : where);
        count($"{kind}: boundary {(A.m_oppositeFace == 0xFFFFFFFF ? "A" : "-")}{(B.m_oppositeFace == 0xFFFFFFFF ? "B" : "-")}", null);
        count($"{kind}: flags {(int)A.m_flags}/{(int)B.m_flags}", null);

        var a0 = V(ma, A.m_a) + posA; var a1 = V(ma, A.m_b) + posA;
        var b0 = V(mb, B.m_a) + posB; var b1 = V(mb, B.m_b) + posB;
        float rev = MathF.Max((a0 - b1).Length(), (a1 - b0).Length());
        float same = MathF.Max((a0 - b0).Length(), (a1 - b1).Length());
        string bucket(float d) => d < 0.001f ? "<1mm" : d < 0.01f ? "<1cm" : d < 0.1f ? "<10cm" : d < 1f ? "<1m" : ">=1m";
        if (rev <= same) count($"{kind}: ends reversed {bucket(rev)}", rev >= 1 ? where : null);
        else count($"{kind}: ends same order {bucket(same)}", where);

        // Collinear overlap, for the edges whose ends don't meet
        if (MathF.Min(rev, same) >= 0.01f)
        {
            float line = MathF.Max(DistToLine(b0, a0, a1), DistToLine(b1, a0, a1));
            var dir = Vector3.Normalize(a1 - a0); float len = (a1 - a0).Length();
            float t0 = Vector3.Dot(b0 - a0, dir), t1 = Vector3.Dot(b1 - a0, dir);
            float overlap = MathF.Min(len, MathF.Max(t0, t1)) - MathF.Max(0, MathF.Min(t0, t1));
            count($"{kind}:   unmatched ends: off line {bucket(line)}, overlap {(overlap > 0.01f ? "yes" : "no")}", where);
            float dy = MathF.Abs((a0.Y + a1.Y) / 2 - (b0.Y + b1.Y) / 2);
            count($"{kind}:   unmatched ends: height gap {bucket(dy)}", null);
        }
    }
}
