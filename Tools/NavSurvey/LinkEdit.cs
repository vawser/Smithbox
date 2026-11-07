using Andre.IO.VFS;
using Havok.hk2018;
using SoulsFormats;
using StudioCore.Editors.MapEditor;
using System.Numerics;
using System.Text.RegularExpressions;

// linkedit [regex] [max] : the Navigation tool's link editing (NavUserEdges) on game navmeshes.
// - the game's files pass NavUserEdges.Check, and no connection names a user edge (so adding and removing needs no remap);
// - a link added between two neighbouring faces passes the check, keeps every cache block the right size, and survives
//   HavokFile.Write and a re-read;
// - removing it again gives back the file byte for byte;
// - removing one of the game's links passes the check, and NavMeshState.Restore gives back the file byte for byte.
static class LinkEdit
{
    public static void Run(VirtualFileSystem fs, string dict, string[] args)
    {
        var re = new Regex(args.Length > 1 ? args[1] : ".");
        int max = args.Length > 2 ? int.Parse(args[2]) : 20;
        var maps = Regex.Matches(dict, @"""/map/m\d\d/(m\d\d_\d\d_\d\d_\d\d)/\1\.nva\.dcx""").Select(m => m.Groups[1].Value)
            .Distinct().Where(m => re.IsMatch(m)).Take(max).ToList();
        var nvcNames = Regex.Matches(dict, @"""/map/nvc/(m\d\d_\d\d_\d\d_\d\d_m\d\d_\d\d_\d\d_\d\d)\.nvc\.dcx""").Select(m => m.Groups[1].Value).Distinct().ToList();
        var profile = Havok.HavokProfile.FromGame("ER");
        var c = new Dictionary<string, long>();
        var examples = new Dictionary<string, string>();
        void Count(string key, string example = null) { c[key] = c.GetValueOrDefault(key) + 1; if (example != null) examples.TryAdd(key, example); }
        var rng = new Random(7);

        foreach (var mapId in maps)
        {
            var map = NavigationReader.Read(fs, mapId, profile, NavigationReader.NvcNamesFor(mapId, nvcNames));
            var bnd = fs.ReadFile(NavigationReader.NvmPath(mapId));
            if (map == null || bnd == null) continue;

            foreach (var f in BND4.Read(bnd.Value.ToArray()).Files)
            {
                var bytes = f.Bytes.ToArray();
                if (DCX.Is(bytes)) bytes = DCX.Decompress(bytes).ToArray();
                var name = Path.GetFileNameWithoutExtension(f.Name);
                var where = $"{mapId}/{name}";

                var doc = Havok.Files.HavokFile.Read(bytes, profile);
                var mesh = doc.ObjectsOf<hkaiNavMesh>().First();
                if (mesh.m_faces.Count < 2) continue;
                var caches = NavUserEdges.CachesOf(mesh);
                Count("files");
                Count($"files with {caches.Count} caches");

                var vanillaError = NavUserEdges.Check(mesh, caches);
                if (vanillaError == null) Count("game file passes the check");
                else { Count("game file fails the check", $"{where}: {vanillaError}"); continue; }

                // Connections never name a user edge
                var entry = map.Nva.Navmeshes.FirstOrDefault(e => map.FileNameOf(e) == name);
                int original = NavUserEdges.OriginalEdgeCount(mesh);
                if (entry == null) Count("file without an NVA entry", where);
                else if (NavUserEdges.ConnectionsNameEdgesFrom(map, entry.NameID, original)) Count("a connection names a user edge", where);
                else Count("no connection names a user edge");

                int mismatchedBefore = NavUserEdges.MismatchedBlocks(mesh, caches);
                if (mismatchedBefore > 0) Count("files with mismatched cache blocks", $"{where}: {mismatchedBefore}");
                var original_ = Havok.Files.HavokFile.Write(doc);
                if (!original_.AsSpan().SequenceEqual(Havok.Files.HavokFile.Write(Havok.Files.HavokFile.Read(original_, profile))))
                {
                    Count("file doesn't write back stably", where);
                    continue;
                }

                // Add a link from a face to a neighbour, or to a random face
                int from = rng.Next(mesh.m_faces.Count);
                int to = -1;
                var fromFace = mesh.m_faces[from];
                for (int e = fromFace.m_startEdgeIndex; e < fromFace.m_startEdgeIndex + fromFace.m_numEdges && to < 0; e++)
                    if (NavigationOverlayBuilder.OppositeFace(mesh, mesh.m_edges[e].m_oppositeFace) is int o && o != from) to = o;
                if (to < 0) to = (from + 1) % mesh.m_faces.Count;

                bool bidirectional = rng.Next(2) == 0;
                var link = new NavUserEdgeLink(from, NavigationOverlayBuilder.FaceCentre(mesh, from), to, NavigationOverlayBuilder.FaceCentre(mesh, to),
                    bidirectional, NavUserEdges.DefaultEdgeData, NavUserEdges.DefaultClearance);
                var aabbBefore = (mesh.m_aabb.m_min, mesh.m_aabb.m_max);
                var added = NavUserEdges.Add(mesh, caches, link);

                if (!added.Ok) { Count($"add refused: {added.Error}", where); continue; }

                var addedError = NavUserEdges.Check(mesh, caches);
                if (addedError == null) Count("after add: passes the check");
                else Count("after add: fails the check", $"{where}: {addedError}");
                if (NavUserEdges.MismatchedBlocks(mesh, caches) == mismatchedBefore) Count("after add: cache blocks keep their sizes");
                else Count("after add: cache blocks broken", where);

                var fwd = mesh.m_edges[added.Edges[0]];
                var back = mesh.m_edges[added.Edges[1]];
                bool shape = fwd.m_oppositeFace == to && back.m_oppositeFace == from && fwd.m_oppositeEdge == added.Edges[1] && back.m_oppositeEdge == added.Edges[0]
                    && ((back.m_flags & hkaiNavMesh.EdgeFlagBits.EDGE_BLOCKED) != 0) == !bidirectional;
                Count(shape ? "after add: the pair is as asked" : "after add: the pair is wrong", shape ? null : where);

                var written = Havok.Files.HavokFile.Write(doc);
                var reread = Havok.Files.HavokFile.Read(written, profile);
                var rereadMesh = reread.ObjectsOf<hkaiNavMesh>().First();
                var rereadError = NavUserEdges.Check(rereadMesh, NavUserEdges.CachesOf(rereadMesh));
                if (rereadError == null && rereadMesh.m_edges.Count == mesh.m_edges.Count && rereadMesh.m_vertices.Count == mesh.m_vertices.Count)
                    Count("after add: survives write and re-read");
                else Count("after add: lost in write", $"{where}: {rereadError}");

                // Removing it gives back the file
                var removed = NavUserEdges.Remove(mesh, caches, added.Edges[0]);
                if (!removed.Ok) { Count($"remove refused: {removed.Error}", where); continue; }
                bool aabbGrew = aabbBefore != (mesh.m_aabb.m_min, mesh.m_aabb.m_max);
                if (aabbGrew) { Count("add grew the bounds (not compared)"); mesh.m_aabb.m_min = aabbBefore.m_min; mesh.m_aabb.m_max = aabbBefore.m_max; }
                if (Havok.Files.HavokFile.Write(doc).AsSpan().SequenceEqual(original_)) Count("add then remove: byte-identical");
                else Count("add then remove: differs", where);

                // Removing one of the game's links, then restoring
                int gameEdge = mesh.m_faces.Where(x => x.m_numUserEdges > 0).Select(x => x.m_startUserEdgeIndex).DefaultIfEmpty(-1).First();
                if (gameEdge < 0) continue;

                var state = new NavMeshState(mesh, caches);
                var removedGame = NavUserEdges.Remove(mesh, caches, gameEdge);
                if (!removedGame.Ok) { Count($"remove game link refused: {removedGame.Error}", where); continue; }
                var gameError = NavUserEdges.Check(mesh, caches);
                if (gameError == null) Count("remove game link: passes the check");
                else Count("remove game link: fails the check", $"{where}: {gameError}");
                if (NavUserEdges.MismatchedBlocks(mesh, caches) <= mismatchedBefore) Count("remove game link: cache blocks keep their sizes");
                else Count("remove game link: cache blocks broken", where);

                state.Restore(mesh);
                if (Havok.Files.HavokFile.Write(doc).AsSpan().SequenceEqual(original_)) Count("restore: byte-identical");
                else Count("restore: differs", where);
            }
        }

        foreach (var (k, v) in c.OrderBy(x => x.Key)) Console.WriteLine($"{v,8}  {k}{(examples.TryGetValue(k, out var ex) ? "   e.g. " + ex : "")}");
    }
}
