using Andre.IO.VFS;
using Havok.hk2018;
using SoulsFormats;
using StudioCore.Editors.MapEditor;
using System.Numerics;
using System.Text.RegularExpressions;

// vertexedit [regex] [max] : the Navigation tool's vertex edits (NavVertexFile, NavVertexEdit.Build) on game maps.
// - building a file with its vertices unmoved keeps the vertices and AABB exactly, and the rebuilt face tree holds
//   every face once, inside its leaf;
// - moving a random vertex with a 3 m radius: the file writes and reads back with the moved positions, the same faces,
//   edges, face and edge data, its clearance cache, the AABB padding it had, and a face tree that holds the moved faces;
// - the validator finds no new errors with every moved file in place (vertex edits keep every index);
// - linking: how far the end points of in-map connected edges are from the nearest vertex of the navmesh they join,
//   which says what link distance keeps joined edges joined.
static class VertexEdit
{
    public static void Run(VirtualFileSystem fs, string dict, string[] args)
    {
        var re = new Regex(args.Length > 1 ? args[1] : ".");
        int max = args.Length > 2 ? int.Parse(args[2]) : 30;
        var maps = Regex.Matches(dict, @"""/map/m\d\d/(m\d\d_\d\d_\d\d_\d\d)/\1\.nva\.dcx""").Select(m => m.Groups[1].Value)
            .Distinct().Where(m => re.IsMatch(m)).Take(max).ToList();
        var nvcNames = Regex.Matches(dict, @"""/map/nvc/(m\d\d_\d\d_\d\d_\d\d_m\d\d_\d\d_\d\d_\d\d)\.nvc\.dcx""").Select(m => m.Groups[1].Value).Distinct().ToList();
        var profile = Havok.HavokProfile.FromGame("ER");
        var c = new Dictionary<string, long>();
        var examples = new Dictionary<string, string>();
        void Count(string key, long n = 1, string example = null) { c[key] = c.GetValueOrDefault(key) + n; if (example != null) examples.TryAdd(key, example); }
        var rng = new Random(7);
        var linkGaps = new List<float>();

        foreach (var mapId in maps)
        {
            var map = NavigationReader.Read(fs, mapId, profile, NavigationReader.NvcNamesFor(mapId, nvcNames));
            if (map?.Nva == null) continue;
            Count("maps");

            int baseErrors = NavigationValidator.Validate(map, _ => null).Count(i => i.Severity == NavIssueSeverity.Error);
            var changes = new List<NavFileChange>();

            foreach (var entry in map.Nva.Navmeshes)
            {
                var file = map.FileOf(entry);
                if (file?.Mesh == null || file.FaceCount == 0 || file.Document == null) continue;
                Count("files");

                var mesh = file.Mesh;
                var edit = new NavVertexFile(entry, file, Matrix4x4.Identity);

                // Unmoved
                var same = NavVertexEdit.Build(file.Document, edit.Vertices);
                var sameMesh = same?.ObjectsOf<hkaiNavMesh>().FirstOrDefault();
                if (sameMesh == null) { Count("unmoved: build failed", 1, file.Name); continue; }
                bool vertsSame = sameMesh.m_vertices.SequenceEqual(mesh.m_vertices);
                bool aabbSame = sameMesh.m_aabb.m_min == mesh.m_aabb.m_min && sameMesh.m_aabb.m_max == mesh.m_aabb.m_max;
                Count(vertsSame && aabbSame ? "unmoved: vertices and AABB exactly kept" : "unmoved: vertices or AABB changed", 1, vertsSame && aabbSame ? null : file.Name);
                var sameTree = Tree(same, sameMesh);
                if (sameTree != null)
                    Count(NavigationTree.Check(sameTree, sameMesh) is string p0 ? "unmoved: face tree problem" : "unmoved: face tree holds every face", 1, NavigationTree.Check(sameTree, sameMesh) is string p1 ? $"{file.Name}: {p1}" : null);
                else
                    Count("unmoved: no face tree (none in the file)");

                // Moved
                int v = rng.Next(edit.Vertices.Length);
                var falloff = edit.Falloff(edit.WorldVertex(v), 0.001f, 3f);
                var offset = new Vector3(0.3f, 0.5f, -0.2f);
                edit.MoveWorld(falloff, offset);
                Count("vertices moved", edit.MovedCount);
                Count(edit.Vertices[v] == edit.Original[v] + offset ? "moved: target took the whole offset" : "moved: target didn't take the whole offset", 1, file.Name);

                var built = NavVertexEdit.Build(file.Document, edit.Vertices);
                if (built == null) { Count("moved: build failed", 1, file.Name); continue; }

                var back = NavModelEdit.Clone(built);
                var backMesh = back.ObjectsOf<hkaiNavMesh>().First();

                bool positions = backMesh.m_vertices.Count == edit.Vertices.Length
                    && backMesh.m_vertices.Select((p, i) => new Vector3(p.X, p.Y, p.Z) == edit.Vertices[i] && p.W == mesh.m_vertices[i].W).All(x => x);
                Count(positions ? "moved: read back with the moved positions" : "moved: positions differ after reading back", 1, positions ? null : file.Name);

                bool topology = backMesh.m_faces.Count == mesh.m_faces.Count && backMesh.m_edges.Count == mesh.m_edges.Count
                    && backMesh.m_faceData.SequenceEqual(mesh.m_faceData) && backMesh.m_edgeData.SequenceEqual(mesh.m_edgeData)
                    && backMesh.m_edges.Select((e, i) => e.m_a == mesh.m_edges[i].m_a && e.m_b == mesh.m_edges[i].m_b && e.m_oppositeFace == mesh.m_edges[i].m_oppositeFace && e.m_flags == mesh.m_edges[i].m_flags).All(x => x);
                Count(topology ? "moved: faces, edges and data kept" : "moved: topology or data changed", 1, topology ? null : file.Name);

                bool cache = (backMesh.m_clearanceCacheSeedingDataSet != null) == (mesh.m_clearanceCacheSeedingDataSet != null);
                Count(cache ? "moved: clearance cache kept" : "moved: clearance cache lost or gained", 1, cache ? null : file.Name);
                if (file.HasClearanceCache)
                    Count(back.ObjectsOf<hkaiNavMeshClearanceCache>().Any() || backMesh.m_clearanceCacheSeedingDataSet != null ? "moved: file with a cache still has one" : "moved: file's cache gone", 1, file.Name);

                var (vMin, vMax) = Bounds(edit.Vertices);
                var (oMin, oMax) = Bounds(edit.Original);
                var padMinBefore = oMin - Xyz(mesh.m_aabb.m_min);
                var padMaxBefore = Xyz(mesh.m_aabb.m_max) - oMax;
                var padMinAfter = vMin - Xyz(backMesh.m_aabb.m_min);
                var padMaxAfter = Xyz(backMesh.m_aabb.m_max) - vMax;
                bool contains = padMinAfter.X >= -1e-4f && padMinAfter.Y >= -1e-4f && padMinAfter.Z >= -1e-4f && padMaxAfter.X >= -1e-4f && padMaxAfter.Y >= -1e-4f && padMaxAfter.Z >= -1e-4f;
                bool padding = Vector3.Distance(Vector3.Max(padMinBefore, Vector3.Zero), padMinAfter) < 1e-3f && Vector3.Distance(Vector3.Max(padMaxBefore, Vector3.Zero), padMaxAfter) < 1e-3f;
                Count(contains ? "moved: AABB holds every vertex" : "moved: AABB misses vertices", 1, contains ? null : file.Name);
                Count(padding ? "moved: AABB padding kept" : "moved: AABB padding changed", 1, padding ? null : $"{file.Name} {padMinBefore} {padMinAfter}");
                Count($"game AABB padding: {(padMinBefore.Length() < 1e-4f && padMaxBefore.Length() < 1e-4f ? "none" : padMinBefore.X < 0 || padMinBefore.Y < 0 || padMinBefore.Z < 0 ? "vertices outside" : "some")}");

                var tree = Tree(back, backMesh);
                if (tree != null)
                {
                    var problem = NavigationTree.Check(tree, backMesh);
                    Count(problem == null ? "moved: face tree holds every face" : "moved: face tree problem", 1, problem == null ? null : $"{file.Name}: {problem}");
                    var mediator = back.ObjectsOf<hkaiStaticTreeNavMeshQueryMediator>().First(m => m.m_tree?.m_treePtr?.m_tree == tree);
                    Count(ReferenceEquals(mediator.m_navMesh, backMesh) ? "moved: mediator names the mesh" : "moved: mediator names another mesh", 1, file.Name);
                }

                changes.Add(new NavFileChange(file.Name, file.Document, back));
            }

            // Every moved file in place at once
            var files = map.Files.ToDictionary(x => x.Key, x => x.Value);
            foreach (var change in changes)
                files[change.Name] = NavigationReader.FromDocument(change.Name, change.After);
            var after = new NavigationMap(map.MapId, map.Nva, files, map.Onav, map.Nvcs);
            var errors = NavigationValidator.Validate(after, _ => null).Where(i => i.Severity == NavIssueSeverity.Error).ToList();
            Count(errors.Count <= baseErrors ? "map: no new validator errors" : "map: new validator errors", 1,
                errors.Count <= baseErrors ? null : $"{mapId}: {baseErrors} -> {errors.Count} {string.Join("; ", errors.Take(3).Select(i => i.Code + " " + string.Join(",", i.Args)))}");

            // Linking: the end points of in-map connected edges, against the nearest vertex of the navmesh they join
            foreach (var view in NavigationRemap.Connections(map).Where(x => !x.IsCrossMap))
            {
                var a = map.FindNavmesh(view.MainNameID);
                var b = map.FindNavmesh(view.TargetNameID);
                var fa = a == null ? null : map.FileOf(a);
                var fb = b == null ? null : map.FileOf(b);
                if (fa?.Mesh == null || fb?.Mesh == null || view.Connection.EdgeIndex >= fa.Mesh.m_edges.Count) { Count("link: connection skipped (missing file or edge)"); continue; }
                if (a.Position != b.Position || a.Rotation != b.Rotation || a.Scale != b.Scale) { Count("link: connection skipped (entries placed differently)"); continue; }

                var edge = fa.Mesh.m_edges[view.Connection.EdgeIndex];
                foreach (var end in new[] { edge.m_a, edge.m_b })
                {
                    var p = Xyz(fa.Mesh.m_vertices[end]);
                    float nearest = fb.Mesh.m_vertices.Min(q => Vector3.Distance(p, Xyz(q)));
                    linkGaps.Add(nearest);
                }
            }
        }

        foreach (var (k, v) in c.OrderBy(x => x.Key)) Console.WriteLine($"{v,12}  {k}{(examples.TryGetValue(k, out var ex) ? "   e.g. " + ex : "")}");

        if (linkGaps.Count > 0)
        {
            Console.WriteLine($"link: {linkGaps.Count} connected edge end points; nearest vertex of the joined navmesh within");
            foreach (var d in new[] { 0.0001f, 0.001f, 0.01f, 0.05f, 0.1f, 0.25f, 0.5f, 1f })
                Console.WriteLine($"{linkGaps.Count(g => g <= d) * 100.0 / linkGaps.Count,10:0.0}%  {d} m");
        }
    }

    static Havok.hk2018.hkcdStaticTree.Aabb6BytesTree Tree(Havok.HavokDocument document, hkaiNavMesh mesh)
        => document.ObjectsOf<hkaiStaticTreeNavMeshQueryMediator>().FirstOrDefault(m => ReferenceEquals(m.m_navMesh, mesh))?.m_tree?.m_treePtr?.m_tree;

    static Vector3 Xyz(Vector4 v) => new(v.X, v.Y, v.Z);

    static (Vector3 Min, Vector3 Max) Bounds(IEnumerable<Vector3> points)
    {
        var min = new Vector3(float.MaxValue);
        var max = new Vector3(float.MinValue);
        foreach (var p in points) { min = Vector3.Min(min, p); max = Vector3.Max(max, p); }
        return (min, max);
    }
}
