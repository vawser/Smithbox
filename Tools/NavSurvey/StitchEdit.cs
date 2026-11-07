using Andre.IO.VFS;
using SoulsFormats;
using StudioCore.Editors.MapEditor;
using System.Text.RegularExpressions;

// stitchedit [regex] [max] : the Navigation tool's re-stitch (NavStitchPlanner) on game maps, with their NVC neighbours.
// - on the game's data: what a plan would remove (should be nothing) and add;
// - after damage (10% of the connection pairs deleted, a few connections pointed at wrong edges): how many deleted
//   connections the re-stitch puts back, whether it finds every damaged one, and the validator's issues before and after;
// - NavConnectionState.Restore gives back the NVA and NVC files byte for byte.
static class StitchEdit
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
        var parameters = new NavStitchParameters();
        bool openOnly = args.Length <= 3 || args[3] != "all";
        var rng = new Random(3);

        foreach (var mapId in maps)
        {
            // Read fresh for each map: the damage below changes the files in memory
            var cache = new Dictionary<string, NavigationMap>();
            NavigationMap Load(string id) => cache.TryGetValue(id, out var m) ? m : cache[id] = NavigationReader.Read(fs, id, profile, NavigationReader.NvcNamesFor(id, nvcNames));
            var map = Load(mapId);
            if (map?.Nva == null) continue;

            // The neighbours share this map's NVC objects, so a plan's cross-map side sees this map's edits
            NavigationMap Neighbour(string id)
            {
                var other = Load(id);
                return other == null ? null : new NavigationMap(other.MapId, other.Nva, other.Files, other.Onav, map.Nvcs.Where(n => n.Opposite(mapId) == id).ToList());
            }

            Count("maps");
            var nvaBytes = map.Nva.Write();
            var nvcBytes = map.Nvcs.ToDictionary(n => n.Name, n => n.Nvc.Write());
            var state = new NavConnectionState(map.Nva, map.Nvcs);
            int issuesBefore = NavigationValidator.Validate(map, Neighbour).Count;

            foreach (var set in new[] { NavmeshSet.N, NavmeshSet.O })
            {
                var plan = NavStitchPlanner.Plan(map, set, Neighbour, parameters, null, openOnly);
                Count($"game data: connections checked", plan.Checked);
                Count($"game data: broken", plan.Broken.Count, plan.Broken.Count > 0 ? $"{mapId} {set} {plan.Broken.Count}" : null);
                foreach (var b in plan.Broken.Take(3))
                    Count($"game data broken: {(b.Nvc == null ? "nva" : "nvc " + b.Nvc.Name)} {b.MainNameID}:{b.Connection.FaceIndex}/{b.Connection.EdgeIndex} -> {b.TargetNameID}:{b.Connection.OppositeFaceIndex}/{b.Connection.OppositeEdgeIndex} gap {plan.BrokenGaps[b.Connection]:0.0}");
                Count($"game data: missing reverse", plan.MissingReverse, plan.MissingReverse > 0 ? $"{mapId} {set}" : null);
                Count($"game data: would add in map", plan.AddedInMap - plan.MissingReverse);
                Count($"game data: would add across maps", plan.AddedCrossMap);
                Count($"game data: neighbours used", plan.Neighbours.Count);
            }

            // Damage: delete 10% of the in-map pairs (both directions) and of the NVC connections, and point a few at wrong edges
            var deleted = new HashSet<(string, int, int, string, int, int)>();
            var pairs = NavStitchPlanner.Stored(map).Where(s => s.Nvc == null).ToList();
            foreach (var s in pairs.Where(_ => rng.NextDouble() < 0.05).ToList())
            {
                foreach (var connector in map.Nva.Connectors)
                    connector.NavMeshConnections.RemoveAll(x =>
                        (connector.MainNameID == s.MainNameID && connector.TargetNameID == s.TargetNameID && x.EdgeIndex == s.Connection.EdgeIndex && x.OppositeEdgeIndex == s.Connection.OppositeEdgeIndex)
                        || (connector.MainNameID == s.TargetNameID && connector.TargetNameID == s.MainNameID && x.EdgeIndex == s.Connection.OppositeEdgeIndex && x.OppositeEdgeIndex == s.Connection.EdgeIndex));
                deleted.Add((mapId, s.MainNameID, s.Connection.EdgeIndex, mapId, s.TargetNameID, s.Connection.OppositeEdgeIndex));
                deleted.Add((mapId, s.TargetNameID, s.Connection.OppositeEdgeIndex, mapId, s.MainNameID, s.Connection.EdgeIndex));
            }
            foreach (var nvc in map.Nvcs)
                foreach (var connector in nvc.Nvc.Connectors)
                    foreach (var x in connector.NavMeshConnections.Where(_ => rng.NextDouble() < 0.1).ToList())
                    {
                        connector.NavMeshConnections.Remove(x);
                        deleted.Add((nvc.MapA, connector.MainNameID, x.EdgeIndex, nvc.MapB, connector.TargetNameID, x.OppositeEdgeIndex));
                    }

            var corrupted = new List<NVA_ER.NavMeshConnection>();
            foreach (var x in map.Nva.Connectors.SelectMany(k => k.NavMeshConnections).Where(_ => rng.NextDouble() < 0.002).Take(5).ToList())
            {
                x.EdgeIndex = x.EdgeIndex + 1_000_000;
                corrupted.Add(x);
            }

            int issuesDamaged = NavigationValidator.Validate(map, Neighbour).Count;
            int restored = 0, brokenFound = 0;

            foreach (var set in new[] { NavmeshSet.N, NavmeshSet.O })
            {
                var plan = NavStitchPlanner.Plan(map, set, Neighbour, parameters, null, openOnly);
                brokenFound += plan.Broken.Count(b => corrupted.Contains(b.Connection));
                Count("damaged: broken found other than the damaged ones", plan.Broken.Count(b => !corrupted.Contains(b.Connection)));
                restored += plan.Added.Count(p => deleted.Contains((p.MapA, p.NameA, p.EdgeA, p.MapB, p.NameB, p.EdgeB)));
                Count("damaged: added that weren't deleted", plan.Added.Count(p => !deleted.Contains((p.MapA, p.NameA, p.EdgeA, p.MapB, p.NameB, p.EdgeB))));
                NavStitchPlanner.Apply(map, plan, true, true);
            }

            int issuesRepaired = NavigationValidator.Validate(map, Neighbour).Count;
            Count("damaged: connections deleted", deleted.Count);
            Count("damaged: of them put back", restored);
            Count("damaged: corrupted", corrupted.Count);
            Count("damaged: of them found broken", brokenFound);
            Count("validator issues: game data", issuesBefore);
            Count("validator issues: damaged", issuesDamaged);
            Count("validator issues: re-stitched", issuesRepaired);
            if (issuesRepaired > issuesBefore) Count("maps with more issues after the re-stitch than the game data", 1, $"{mapId}: {issuesBefore} -> {issuesRepaired}");

            // A second plan has nothing left to remove, and nothing new to add
            foreach (var set in new[] { NavmeshSet.N, NavmeshSet.O })
            {
                var again = NavStitchPlanner.Plan(map, set, Neighbour, parameters, null, openOnly);
                Count(again.IsEmpty ? "second plan: empty" : "second plan: not empty", 1, again.IsEmpty ? null : $"{mapId} {set}: {again.Broken.Count} broken, {again.Added.Count} added");
            }

            // Restore gives back the files
            state.Restore();
            bool same = map.Nva.Write().AsSpan().SequenceEqual(nvaBytes) && map.Nvcs.All(n => n.Nvc.Write().AsSpan().SequenceEqual(nvcBytes[n.Name]));
            Count(same ? "restore: byte-identical" : "restore: differs", 1, same ? null : mapId);
        }

        foreach (var (k, v) in c.OrderBy(x => x.Key)) Console.WriteLine($"{v,9}  {k}{(examples.TryGetValue(k, out var ex) ? "   e.g. " + ex : "")}");
    }
}
