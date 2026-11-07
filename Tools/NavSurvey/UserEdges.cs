using Andre.IO.VFS;
using Havok.hk2018;
using SoulsFormats;
using System.Text.RegularExpressions;

// useredges [regex] [max] : how the game's navmeshes lay out their user edges (jump, ladder and drop links), so the
// Navigation tool can add and remove them the same way: where they sit in the edge array, which vertices they use,
// how they point at their target, and what else in the file is sized by edges or vertices.
static class UserEdges
{
    public static void Run(VirtualFileSystem fs, string dict, string[] args)
    {
        var re = new Regex(args.Length > 1 ? args[1] : ".");
        int max = args.Length > 2 ? int.Parse(args[2]) : 50;
        var maps = Regex.Matches(dict, @"""/map/m\d\d/(m\d\d_\d\d_\d\d_\d\d)/\1\.nva\.dcx""").Select(m => m.Groups[1].Value)
            .Distinct().Where(m => re.IsMatch(m)).Take(max).ToList();
        var profile = Havok.HavokProfile.FromGame("ER");
        var c = new Dictionary<string, long>();
        void Count(string key, long n = 1) => c[key] = c.GetValueOrDefault(key) + n;
        var examples = new Dictionary<string, string>();
        void Example(string key, string text) { Count(key); examples.TryAdd(key, text); }

        foreach (var mapId in maps)
        {
            var bnd = fs.ReadFile(StudioCore.Editors.MapEditor.NavigationReader.NvmPath(mapId));
            if (bnd == null) continue;

            foreach (var f in BND4.Read(bnd.Value.ToArray()).Files)
            {
                var bytes = f.Bytes.ToArray();
                if (DCX.Is(bytes)) bytes = DCX.Decompress(bytes).ToArray();
                var doc = Havok.Files.HavokFile.Read(bytes, profile);
                var mesh = doc.ObjectsOf<hkaiNavMesh>().First();
                if (mesh.m_faces.Count == 0) continue;
                var name = $"{mapId}/{Path.GetFileNameWithoutExtension(f.Name)}";
                Count("files");

                int orig = mesh.m_faces.Sum(x => x.m_numEdges);
                int user = mesh.m_faces.Sum(x => x.m_numUserEdges);
                if (orig + user == mesh.m_edges.Count) Count("edges = original + user");
                else Example("edges != original + user", $"{name} {mesh.m_edges.Count} vs {orig}+{user}");

                // Original edge ranges in face order from 0
                int next = 0; bool ordered = true;
                foreach (var face in mesh.m_faces) { if (face.m_startEdgeIndex != next) ordered = false; next += face.m_numEdges; }
                Count(ordered ? "original ranges packed in face order" : "original ranges not packed");

                CacheFacts(mesh, doc, name, user, Count, Example);
                if (user == 0) { Count("files without user edges"); continue; }
                Count("files with user edges");
                Count("user edges", user);

                // User edge ranges: all after the original edges, packed in face order?
                next = orig; bool userOrdered = true; bool userAfter = true;
                foreach (var face in mesh.m_faces)
                {
                    if (face.m_numUserEdges == 0) continue;
                    if (face.m_startUserEdgeIndex < orig) userAfter = false;
                    if (face.m_startUserEdgeIndex != next) userOrdered = false;
                    next = face.m_startUserEdgeIndex + face.m_numUserEdges;
                }
                Count(userAfter ? "user ranges after originals" : "user ranges among originals");
                if (userOrdered) Count("user ranges packed in face order");
                else Example("user ranges not packed in face order", name);

                // Faces with no user edges: what their start user index is
                foreach (var face in mesh.m_faces.Where(x => x.m_numUserEdges == 0).Take(1))
                    Example($"empty user start = {(face.m_startUserEdgeIndex == -1 ? "-1" : face.m_startUserEdgeIndex == 0 ? "0" : "other")}", $"{name} {face.m_startUserEdgeIndex}");

                var faceVertices = new HashSet<int>();
                var origVertices = new HashSet<int>();
                for (int k = 0; k < orig && k < mesh.m_edges.Count; k++) { origVertices.Add(mesh.m_edges[k].m_a); origVertices.Add(mesh.m_edges[k].m_b); }
                if (origVertices.Count == mesh.m_vertices.Count) Count("every vertex used by an original edge");
                else Example("vertices only user edges use", $"{name} {mesh.m_vertices.Count - origVertices.Count} of {mesh.m_vertices.Count}");

                var cache = doc.ObjectsOf<hkaiNavMeshClearanceCache>().FirstOrDefault();
                if (cache != null)
                {
                    Count("clearance caches");
                    if (cache.m_vertexClearances.Count == mesh.m_vertices.Count) Count("cache vertexClearances = vertices");
                    else Example("cache vertexClearances != vertices", $"{name} {cache.m_vertexClearances.Count} vs {mesh.m_vertices.Count}");
                    if (cache.m_faceOffsets.Count == mesh.m_faces.Count || cache.m_faceOffsets.Count == mesh.m_faces.Count + 1) Count("cache faceOffsets ~ faces");
                    else Example("cache faceOffsets other", $"{name} {cache.m_faceOffsets.Count} vs {mesh.m_faces.Count}");
                    if (cache.m_mcpData.Count > 0) Example("cache mcpData nonempty", $"{name} {cache.m_mcpData.Count} (edges {mesh.m_edges.Count}, orig {orig})");
                }

                for (int fi = 0; fi < mesh.m_faces.Count; fi++)
                {
                    var face = mesh.m_faces[fi];
                    if (face.m_numUserEdges == 0) continue;
                    if (face.m_numUserEdges > 1) Count($"faces with {Math.Min((int)face.m_numUserEdges, 5)}{(face.m_numUserEdges >= 5 ? "+" : "")} user edges");

                    var own = new List<(int A, int B)>();
                    for (int k = face.m_startEdgeIndex; k < face.m_startEdgeIndex + face.m_numEdges; k++) own.Add((mesh.m_edges[k].m_a, mesh.m_edges[k].m_b));
                    var ownVerts = own.SelectMany(e => new[] { e.A, e.B }).ToHashSet();

                    for (int k = face.m_startUserEdgeIndex; k < face.m_startUserEdgeIndex + face.m_numUserEdges; k++)
                    {
                        var e = mesh.m_edges[k];
                        Count($"flags {(int)e.m_flags}");
                        if (e.m_userEdgeCost != 0) Example("userEdgeCost nonzero", $"{name} {e.m_userEdgeCost}");
                        if (mesh.m_edgeDataStriding > 0) Count($"edgeData {mesh.m_edgeData[k * mesh.m_edgeDataStriding]:X}");

                        if (own.Contains((e.m_a, e.m_b))) Count("verts = an own edge (same order)");
                        else if (own.Contains((e.m_b, e.m_a))) Count("verts = an own edge (reversed)");
                        else if (ownVerts.Contains(e.m_a) && ownVerts.Contains(e.m_b)) Example("verts = two own corners, not an edge", $"{name} face {fi} edge {k}");
                        else if (e.m_a == e.m_b) Example("verts a == b", $"{name} face {fi} edge {k}");
                        else if (!origVertices.Contains(e.m_a) || !origVertices.Contains(e.m_b)) Example("verts own to user edges", $"{name} face {fi} edge {k}");
                        else Example("verts elsewhere", $"{name} face {fi} edge {k}: {e.m_a},{e.m_b}");

                        Count($"oppositeFace section {e.m_oppositeFace >> 22:X}");
                        int target = (int)(e.m_oppositeFace & 0x3FFFFF);
                        if (target == fi) Example("targets own face", $"{name} face {fi}");

                        if (e.m_oppositeEdge == 0xFFFFFFFF) { Count("oppositeEdge none"); continue; }
                        Count($"oppositeEdge section {e.m_oppositeEdge >> 22:X}");
                        int oe = (int)(e.m_oppositeEdge & 0x3FFFFF);
                        if (oe >= mesh.m_edges.Count) { Example("oppositeEdge out of range", name); continue; }
                        var tf = target < mesh.m_faces.Count ? mesh.m_faces[target] : null;
                        bool inTargetUser = tf != null && oe >= tf.m_startUserEdgeIndex && oe < tf.m_startUserEdgeIndex + tf.m_numUserEdges;
                        bool inTargetOrig = tf != null && oe >= tf.m_startEdgeIndex && oe < tf.m_startEdgeIndex + tf.m_numEdges;
                        if (inTargetUser)
                        {
                            Count("oppositeEdge = a user edge of the target");
                            var back = mesh.m_edges[oe];
                            if ((back.m_oppositeEdge & 0x3FFFFF) == k && (back.m_oppositeFace & 0x3FFFFF) == fi) Count("  ...which points back");
                            else Example("  ...which doesn't point back", $"{name} face {fi} edge {k} -> {oe}");
                            if (back.m_a == e.m_b && back.m_b == e.m_a) Count("  ...with reversed verts");
                            else if (back.m_a == e.m_a && back.m_b == e.m_b) Count("  ...with the same verts");
                            else Count("  ...with other verts");
                            if (back.m_flags != e.m_flags) Example("  ...with other flags", $"{name} {(int)e.m_flags} vs {(int)back.m_flags}");
                            if (mesh.m_edgeDataStriding > 0 && mesh.m_edgeData[oe] != mesh.m_edgeData[k]) Example("  ...with other edgeData", $"{name} {mesh.m_edgeData[k]:X} vs {mesh.m_edgeData[oe]:X}");
                        }
                        else if (inTargetOrig) Example("oppositeEdge = an original edge of the target", $"{name} face {fi} edge {k}");
                        else Example("oppositeEdge elsewhere", $"{name} face {fi} edge {k} -> {oe}");
                    }
                }

                Geometry(mesh, doc, name, orig, Count, Example);

                var setups = doc.ObjectsOf<hkaiUserEdgeSetupArray>().FirstOrDefault();
                if (setups != null) Example("files with hkaiUserEdgeSetupArray", $"{name} {setups.m_edgeSetups.Count} setups, {user} user edges");
            }
        }

        foreach (var (k, v) in c.OrderBy(x => x.Key).Where(x => !x.Key.StartsWith("edgeData "))) Console.WriteLine($"{v,10}  {k}{(examples.TryGetValue(k, out var ex) ? "   e.g. " + ex : "")}");
    }

    static System.Numerics.Vector3 V(hkaiNavMesh m, int i) => new(m.m_vertices[i].X, m.m_vertices[i].Y, m.m_vertices[i].Z);

    // Where a user edge's vertices sit, what the clearance cache holds for them, and how the pairs relate
    static void Geometry(hkaiNavMesh mesh, Havok.HavokDocument doc, string name, int orig, Action<string, long> count, Action<string, string> example)
    {
        var userVertexUse = new Dictionary<int, int>();
        var caches = doc.ObjectsOf<hkaiNavMeshClearanceCache>().ToList();
        count($"clearance caches per file {caches.Count}", 1);

        for (int fi = 0; fi < mesh.m_faces.Count; fi++)
        {
            var face = mesh.m_faces[fi];
            foreach (var cache in caches.Take(1))
            {
                if (cache.m_faceOffsets.Count <= fi + 1) continue;
                uint o0 = cache.m_faceOffsets[fi], o1 = cache.m_faceOffsets[fi + 1];
                if (o0 == 0xFFFFFFFF || o1 == 0xFFFFFFFF || o1 < o0) { count("cache face offset unset", 1); continue; }
                long size = o1 - o0;
                int n = face.m_numEdges, u = face.m_numUserEdges;
                if (n <= 6) count($"cache face n{n} u{(u == 0 ? "0" : "1+")} size {size}", 1);
            }

            for (int k = face.m_startUserEdgeIndex; k < face.m_startUserEdgeIndex + face.m_numUserEdges; k++)
            {
                var e = mesh.m_edges[k];
                userVertexUse[e.m_a] = userVertexUse.GetValueOrDefault(e.m_a) + 1;
                userVertexUse[e.m_b] = userVertexUse.GetValueOrDefault(e.m_b) + 1;
                var a = V(mesh, e.m_a); var b = V(mesh, e.m_b);

                // On one of the face's own edges?
                bool onOwn = false;
                for (int j = face.m_startEdgeIndex; j < face.m_startEdgeIndex + face.m_numEdges; j++)
                {
                    var p = V(mesh, mesh.m_edges[j].m_a); var q = V(mesh, mesh.m_edges[j].m_b);
                    if (DistToSegment(a, p, q) < 0.01f && DistToSegment(b, p, q) < 0.01f) { onOwn = true; break; }
                }
                if (onOwn) count("user verts on an own edge", 1);
                else if (InsideXZ(mesh, face, a) && InsideXZ(mesh, face, b)) count("user verts inside the face (XZ)", 1);
                else example("user verts off the face", $"{name} face {fi} edge {k}");

                if ((a - b).Length() < 0.001f) count("user edge zero length", 1);

                if (e.m_oppositeEdge != 0xFFFFFFFF)
                {
                    var back = mesh.m_edges[(int)(e.m_oppositeEdge & 0x3FFFFF)];
                    var ba = V(mesh, back.m_a); var bb = V(mesh, back.m_b);
                    if ((ba - b).Length() < 0.01f && (bb - a).Length() < 0.01f) count("pair verts coincide reversed", 1);
                    else if ((ba - a).Length() < 0.01f && (bb - b).Length() < 0.01f) count("pair verts coincide same", 1);
                    else count("pair verts apart", 1);
                    count($"pair flags {(int)e.m_flags}/{(int)back.m_flags}", 1);
                    if (mesh.m_edgeDataStriding > 0)
                    {
                        int d0 = mesh.m_edgeData[k], d1 = mesh.m_edgeData[(int)(e.m_oppositeEdge & 0x3FFFFF)];
                        count(d0 == d1 ? "pair edgeData same" : (d0 & 0xFF) == (d1 & 0xFF) ? "pair edgeData same low byte" : "pair edgeData other", 1);
                    }
                }

                foreach (var cache in caches.Take(1))
                {
                    if (cache.m_mcpData.Count > k) count($"mcp user {cache.m_mcpData[k].m_interpolant:X2}/{cache.m_mcpData[k].m_clearance:X2}", 1);
                    if (cache.m_vertexClearances.Count > e.m_a) count($"vertexClearance user {(cache.m_vertexClearances[e.m_a] == 0xFF ? "FF" : cache.m_vertexClearances[e.m_a] == 0 ? "00" : "other")}", 1);
                }
            }
        }

        foreach (var uses in userVertexUse.Values) count($"user vertex used by {Math.Min(uses, 3)}{(uses >= 3 ? "+" : "")} edges", 1);
        foreach (var cache in caches.Take(1))
            for (int k = 0; k < orig && k < cache.m_mcpData.Count; k += 97) count($"mcp orig {(cache.m_mcpData[k].m_clearance == 0xFF ? "FF" : "other")}", 1);

        // Are user vertices appended after all the original edge vertices?
        int maxOrig = mesh.m_edges.Take(orig).Max(x => Math.Max(x.m_a, x.m_b));
        int minUser = userVertexUse.Count > 0 ? userVertexUse.Keys.Min() : int.MaxValue;
        count(minUser > maxOrig ? "user vertices after original ones" : "user vertices interleaved", 1);
    }

    static float DistToSegment(System.Numerics.Vector3 p, System.Numerics.Vector3 a, System.Numerics.Vector3 b)
    {
        var ab = b - a; float t = ab.LengthSquared() < 1e-12f ? 0 : Math.Clamp(System.Numerics.Vector3.Dot(p - a, ab) / ab.LengthSquared(), 0, 1);
        return (a + ab * t - p).Length();
    }

    static bool InsideXZ(hkaiNavMesh mesh, hkaiNavMesh.Face face, System.Numerics.Vector3 p)
    {
        bool inside = false;
        for (int j = face.m_startEdgeIndex; j < face.m_startEdgeIndex + face.m_numEdges; j++)
        {
            var a = V(mesh, mesh.m_edges[j].m_a); var b = V(mesh, mesh.m_edges[j].m_b);
            if ((a.Z > p.Z) != (b.Z > p.Z) && p.X < (b.X - a.X) * (p.Z - a.Z) / (b.Z - a.Z) + a.X) inside = !inside;
        }
        return inside;
    }

    // What the clearance caches hold, and how a face's block of edge pair clearances is sized
    static void CacheFacts(hkaiNavMesh mesh, Havok.HavokDocument doc, string name, int user, Action<string, long> count, Action<string, string> example)
    {
        var caches = doc.ObjectsOf<hkaiNavMeshClearanceCache>().ToList();
        string kind = user > 0 ? "with user edges" : "without user edges";
        count($"cache: files {kind}, {caches.Count} caches, seeding {(mesh.m_clearanceCacheSeedingDataSet != null ? "set" : "none")}", 1);
        if (mesh.m_clearanceCacheSeedingDataSet != null)
            foreach (var d in mesh.m_clearanceCacheSeedingDataSet.m_cacheDatas)
                example($"cache: seeding data id {d.m_id:X} info {d.m_info:X} mask {d.m_infoMask:X}", name);

        for (int ci = 0; ci < caches.Count; ci++)
        {
            var cache = caches[ci];
            count($"cache[{ci}]: faceOffsets {(cache.m_faceOffsets.Count == mesh.m_faces.Count ? "= faces" : cache.m_faceOffsets.Count == mesh.m_faces.Count + 1 ? "= faces + 1" : "other")}", 1);
            count($"cache[{ci}]: last offset {(cache.m_faceOffsets.Count > 0 && cache.m_faceOffsets[^1] == cache.m_edgePairClearances.Count ? "= pair count" : "other")}", 1);
            count($"cache[{ci}]: unused {(cache.m_unusedEdgePairElements == 0 ? "0" : "other")}, uncalculated {(cache.m_uncalculatedFacesLowerBound == 0 ? "0" : "other")}", 1);
            example($"cache[{ci}]: ceiling {cache.m_clearanceCeiling} mult {cache.m_clearanceIntToRealMultiplier}", name);
            if (ci == 1 && caches[0].m_edgePairClearances.SequenceEqual(cache.m_edgePairClearances) && caches[0].m_mcpData.Count == cache.m_mcpData.Count)
                count("cache[1] = cache[0]", 1);

            if (ci > 0) continue;
            for (int fi = 0; fi + 1 < cache.m_faceOffsets.Count && fi < mesh.m_faces.Count; fi++)
            {
                var face = mesh.m_faces[fi];
                long size = (long)cache.m_faceOffsets[fi + 1] - cache.m_faceOffsets[fi];
                int k = 0;
                for (int e = face.m_startEdgeIndex; e < face.m_startEdgeIndex + face.m_numEdges; e++)
                    if (mesh.m_edges[e].m_oppositeFace != 0xFFFFFFFF) k++;
                int inner = k;
                for (int e = face.m_startUserEdgeIndex; e < face.m_startUserEdgeIndex + face.m_numUserEdges; e++)
                    if (mesh.m_edges[e].m_oppositeFace != 0xFFFFFFFF) k++;
                int all = face.m_numEdges + face.m_numUserEdges;
                string u = face.m_numUserEdges > 0 ? "user" : "plain";
                if (size == k * (k - 1) / 2 + 1) count($"cache size = T(k)+1, k = linked edges ({u})", 1);
                else if (size == all * (all - 1) / 2 + 1) count($"cache size = T(all edges)+1 ({u})", 1);
                else if (size == k * (k - 1) / 2) count($"cache size = T(k), k = linked edges ({u})", 1);
                else example($"cache size other ({u})", $"{name} face {fi}: size {size}, linked {k} (inner {inner}), all {all}");
                if (size > 0 && fi == 0) count($"cache first byte {cache.m_edgePairClearances[(int)cache.m_faceOffsets[fi]]:X2}", 1);
            }
        }
    }
}
