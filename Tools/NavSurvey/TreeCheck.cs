using Andre.IO.VFS;
using Havok.hk2018;
using SoulsFormats;
using StudioCore.Editors.MapEditor;
using System.Text.RegularExpressions;

// treecheck [regex] [max] : NavigationTree's decoder against the game's navmesh face trees (every face once, inside its
// leaf), and its builder on the same meshes (the same check, and the node count against the game's).
static class TreeCheck
{
    public static void Run(VirtualFileSystem fs, string dict, string[] args)
    {
        var re = new Regex(args.Length > 1 ? args[1] : ".");
        int max = args.Length > 2 ? int.Parse(args[2]) : 50;
        var maps = Regex.Matches(dict, @"""/map/m\d\d/(m\d\d_\d\d_\d\d_\d\d)/\1\.nva\.dcx""").Select(m => m.Groups[1].Value)
            .Distinct().Where(m => re.IsMatch(m)).Take(max).ToList();
        var profile = Havok.HavokProfile.FromGame("ER");
        var c = new Dictionary<string, long>();
        var examples = new Dictionary<string, string>();
        void Count(string key, long n = 1, string example = null) { c[key] = c.GetValueOrDefault(key) + n; if (example != null) examples.TryAdd(key, example); }

        foreach (var mapId in maps)
        {
            var map = NavigationReader.Read(fs, mapId, profile, Array.Empty<string>());
            if (map == null) continue;
            foreach (var file in map.Files.Values.Where(f => f.FaceCount > 0))
            {
                var mediator = file.Document.ObjectsOf<hkaiStaticTreeNavMeshQueryMediator>().FirstOrDefault();
                var tree = mediator?.m_tree?.m_treePtr?.m_tree;
                if (tree == null) { Count("files without a face tree", 1, file.Name); continue; }
                Count("files");
                Count(ReferenceEquals(mediator.m_navMesh, file.Mesh) ? "mediator names the file's mesh" : "mediator names another mesh");

                var game = NavigationTree.Check(tree, file.Mesh);
                Count(game == null ? "game tree: every face once, inside its leaf" : "game tree: problem", 1, game == null ? null : $"{map.MapId}/{file.Name}: {game}");
                var nodes = NavigationTree.Decode(tree);
                int leaves = nodes.Count(n => n.IsLeaf);
                Count(leaves == file.FaceCount ? "game tree: one leaf per face" : "game tree: leaves != faces", 1, leaves == file.FaceCount ? null : $"{file.Name} {leaves} vs {file.FaceCount}");
                Count(tree.m_nodes.Count == 2 * file.FaceCount - 1 ? "game tree: 2n-1 nodes" : "game tree: other node count", 1, $"{file.Name} {tree.m_nodes.Count} for {file.FaceCount}");

                var built = NavigationTree.Build(file.Mesh);
                var mine = NavigationTree.Check(built, file.Mesh);
                Count(mine == null ? "built tree: every face once, inside its leaf" : "built tree: problem", 1, mine == null ? null : $"{map.MapId}/{file.Name}: {mine}");
                Count("built tree nodes", built.m_nodes.Count);
                Count("game tree nodes", tree.m_nodes.Count);

                // Tightness: the mean leaf box volume against the face's own
                foreach (var (name, t) in new[] { ("game", tree), ("built", built) })
                {
                    var bounds = NavigationTree.FaceBounds(file.Mesh);
                    double slack = 0; int n = 0;
                    foreach (var leaf in NavigationTree.Decode(t).Where(x => x.IsLeaf && x.Face < bounds.Length))
                    {
                        var e = leaf.Max - leaf.Min; var fe = bounds[leaf.Face].Max - bounds[leaf.Face].Min;
                        slack += (e.X - fe.X) + (e.Y - fe.Y) + (e.Z - fe.Z); n++;
                    }
                    Count($"{name} leaf slack (mm, summed)", (long)(slack * 1000));
                }
            }
        }

        foreach (var (k, v) in c.OrderBy(x => x.Key)) Console.WriteLine($"{v,12}  {k}{(examples.TryGetValue(k, out var ex) ? "   e.g. " + ex : "")}");
    }
}
