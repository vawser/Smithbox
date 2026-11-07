using Andre.IO.VFS;
using SoulsFormats;
using StudioCore.Editors.MapEditor;
using System.Numerics;
using System.Text.RegularExpressions;

// gateedit [regex] [max] : the Navigation tool's gate edits (NavGates) on game maps.
// - the validator's gate checks on the game's data (these are inherited, but should be few);
// - the walked costs (NavGates.WalkingDistances) against the game's costs, for the game's own gates;
// - Rebuild of every navmesh against the game's gates: how many per pair, how near each game gate a rebuilt one is,
//   whether the validator finds anything new, and that NavGateState.Restore gives back the NVA byte for byte;
// - Move, SetCost, Remove and Add on a gate: the partner follows, the table stays symmetric, the restore is exact.
// Maps with a rotated navmesh entry are skipped (their mesh isn't in map space here).
static class GateEdit
{
    public static void Run(VirtualFileSystem fs, string dict, string[] args)
    {
        var re = new Regex(args.Length > 1 ? args[1] : ".");
        int max = args.Length > 2 ? int.Parse(args[2]) : 2000;
        var maps = Regex.Matches(dict, @"""/map/m\d\d/(m\d\d_\d\d_\d\d_\d\d)/\1\.nva\.dcx""").Select(m => m.Groups[1].Value)
            .Distinct().Where(m => re.IsMatch(m)).Take(max).ToList();
        var profile = Havok.HavokProfile.FromGame("ER");
        var c = new Dictionary<string, long>();
        var examples = new Dictionary<string, string>();
        void Count(string key, long n = 1, string example = null) { c[key] = c.GetValueOrDefault(key) + n; if (example != null) examples.TryAdd(key, example); }
        var stats = new Dictionary<string, List<float>>();
        void Stat(string key, float v) { if (!stats.TryGetValue(key, out var l)) stats[key] = l = new(); l.Add(v); }
        Matrix4x4 ToMap(NVA_ER.Navmesh e) => Matrix4x4.Identity;
        var rng = new Random(7);

        foreach (var mapId in maps)
        {
            var map = NavigationReader.Read(fs, mapId, profile, Array.Empty<string>());
            var nva = map?.Nva;
            if (nva == null) continue;
            var list = nva.Navmeshes;
            if (list.All(e => e.GateNodes.Count == 0)) { Count("maps without gates"); continue; }
            if (list.Any(e => e.Rotation != Vector3.Zero || e.Position != Vector3.Zero)) { Count("maps skipped: a rotated entry", 1, mapId); continue; }
            Count("maps with gates");

            // The game's data
            var baseline = NavigationValidator.Validate(map);
            foreach (var issue in baseline.Where(i => i.Code.ToString().StartsWith("Gate")))
                Count($"game: {issue.Severity} {issue.Code}", 1, $"{mapId} {string.Join(",", issue.Args)}");

            // Walked costs against the game's
            foreach (var e in list.Where(e => e.GateNodes.Count >= 2))
            {
                var mesh = map.FileOf(e)?.Mesh;
                if (mesh == null) continue;
                var walk = NavGates.WalkingDistances(mesh, ToMap(e), e.GateNodes.Select(g => g.Position).ToList());
                for (int i = 0; i < e.GateNodes.Count; i++)
                    for (int j = i + 1; j < e.GateNodes.Count; j++)
                    {
                        if (j >= e.GateNodes[i].NeighbourGateNodeCosts.Count) continue;
                        short game = e.GateNodes[i].NeighbourGateNodeCosts[j];
                        float w = walk[i, j];
                        string s = NavigationMap.SetOf(e).ToString();
                        if (game == NavGates.Unreachable) Count(float.IsFinite(w) ? $"cost {s}: game unreachable, walked reachable" : $"cost {s}: both unreachable", 1, $"{mapId} {e.NameID} {i}-{j}");
                        else if (!float.IsFinite(w)) Count($"cost {s}: game reachable, walked unreachable", 1, $"{mapId} {e.NameID} {i}-{j}");
                        else
                        {
                            Count($"cost {s}: both reachable");
                            float gm = (ushort)game / 100f;
                            if (w > 0.01f) Stat($"cost {s}: game / walked", gm / w);
                            Stat($"cost {s}: walked - game (m)", w - gm);
                        }
                    }
            }

            // Which site rule and join distance place the most gates where the game has them
            {
                var tryState = new NavGateState(nva);
                var gamePairs = list.SelectMany(e => e.GateNodes.Select(g => (Owner: e, T: (int)g.ConnectedNavmeshIndex, P: g.Position))).ToList();
                foreach (var join in new[] { 2f, 5f, 10f })
                    {
                        NavGates.Rebuild(map, list, ToMap, join);
                        int exact = 0, near = 0, sameCount = 0, pairs = 0;
                        foreach (var g in gamePairs)
                        {
                            var d = g.Owner.GateNodes.Where(x => x.ConnectedNavmeshIndex == g.T).Select(x => Vector3.Distance(x.Position, g.P)).DefaultIfEmpty(float.MaxValue).Min();
                            if (d < 0.01f) exact++;
                            if (d < 1f) near++;
                        }
                        foreach (var grp in gamePairs.GroupBy(x => (x.Owner, x.T)))
                        {
                            pairs++;
                            if (grp.Key.Owner.GateNodes.Count(x => x.ConnectedNavmeshIndex == grp.Key.T) == grp.Count()) sameCount++;
                        }
                        string key = $"try join {join,4} m";
                        Count($"{key}: game gates", gamePairs.Count);
                        Count($"{key}: matched within 1 cm", exact);
                        Count($"{key}: matched within 1 m", near);
                        Count($"{key}: pairs with the game's gate count", sameCount);
                        Count($"{key}: pairs", pairs);
                        tryState.Restore();
                    }
            }

            // Rebuild everything
            var before = nva.Write();
            var gameGates = list.SelectMany(e => e.GateNodes.Select(g => (Owner: e, Gate: NavGateState.Copy(g)))).ToList();
            var state = new NavGateState(nva);
            var rebuilt = NavGates.Rebuild(map, list, ToMap);
            Count("rebuild: gates removed", rebuilt.Removed);
            Count("rebuild: gates added", rebuilt.Added);

            foreach (var e in list)
            {
                string s = NavigationMap.SetOf(e).ToString();
                var byTarget = e.GateNodes.GroupBy(g => (int)g.ConnectedNavmeshIndex).ToDictionary(g => g.Key, g => g.ToList());
                var gameByTarget = gameGates.Where(x => x.Owner == e).GroupBy(x => (int)x.Gate.ConnectedNavmeshIndex).ToDictionary(g => g.Key, g => g.Select(x => x.Gate).ToList());
                foreach (var t in byTarget.Keys.Union(gameByTarget.Keys))
                {
                    int ours = byTarget.GetValueOrDefault(t)?.Count ?? 0, theirs = gameByTarget.GetValueOrDefault(t)?.Count ?? 0;
                    Count(ours == theirs ? $"rebuild {s}: same gate count to a neighbour" : ours < theirs ? $"rebuild {s}: fewer gates to a neighbour" : $"rebuild {s}: more gates to a neighbour");
                    foreach (var g in gameByTarget.GetValueOrDefault(t) ?? new())
                    {
                        var near = (byTarget.GetValueOrDefault(t) ?? new()).Select(x => Vector3.Distance(x.Position, g.Position)).DefaultIfEmpty(float.MaxValue).Min();
                        Stat($"rebuild {s}: distance from a game gate to the nearest rebuilt gate of the same pair (m)", Math.Min(near, 1000));
                        Count(near < 0.01f ? $"rebuild {s}: game gate matched within 1 cm" : near < 1f ? $"rebuild {s}: game gate matched within 1 m" : near < 1000 ? $"rebuild {s}: game gate matched further" : $"rebuild {s}: game gate has no rebuilt gate to its pair");
                    }
                }
                for (int g = 0; g < e.GateNodes.Count; g++)
                    Count($"rebuild {s}: status {NavGates.Status(map, e, g)}");
                if (e.GateNodes.Count > NavGates.MaxGates) Count("rebuild: more than 16 gates", 1, mapId);
            }

            var issues = NavigationValidator.MarkInherited(NavigationValidator.Validate(map), baseline);
            foreach (var issue in issues.Where(i => !i.Inherited))
                Count($"rebuild: new {issue.Severity} {issue.Code}", 1, $"{mapId} {NavigationValidator.Describe(issue)}");

            // Written and read back
            var written = NVA_ER.Read(nva.Write());
            Count(written.Navmeshes.Sum(e => e.GateNodes.Count) == list.Sum(e => e.GateNodes.Count) ? "rebuild: written and read back" : "rebuild: write lost gates", 1, mapId);

            state.Restore();
            Count(before.AsSpan().SequenceEqual(nva.Write()) ? "rebuild: restore byte-identical" : "rebuild: restore differs", 1, mapId);

            // Edits on one paired gate
            var paired = list.SelectMany(e => Enumerable.Range(0, e.GateNodes.Count).Select(g => (e, g))).Where(x => NavGates.Status(map, x.e, x.g) == NavGateStatus.Paired).ToList();
            if (paired.Count == 0) continue;
            var (owner, gate) = paired[rng.Next(paired.Count)];
            var partner = NavGates.Partner(map, owner, gate).Value;
            var edit = new NavGateState(nva);

            var moved = NavGates.Move(map, owner, gate, owner.GateNodes[gate].Position + new Vector3(1, 0, 1), ToMap);
            var p1 = owner.GateNodes[gate].Position;
            var p2 = partner.Entry.GateNodes[partner.Gate].Position;
            Count(moved.Ok && p1 == p2 ? "move: the partner follows" : "move: the partner doesn't follow", 1, mapId);
            var target = list[owner.GateNodes[gate].ConnectedNavmeshIndex];
            var snap = NavGates.Snap(map, owner, target, p1, ToMap);
            Count(snap != null && Vector3.Distance(snap.Value, p1) < 1e-3f ? "move: on a connection edge" : "move: off the connection edges", 1, mapId);

            if (owner.GateNodes.Count >= 2)
            {
                int other = gate == 0 ? 1 : 0;
                NavGates.SetCost(owner, gate, other, 1234);
                Count(owner.GateNodes[gate].NeighbourGateNodeCosts[other] == 1234 && owner.GateNodes[other].NeighbourGateNodeCosts[gate] == 1234 ? "set cost: both ways" : "set cost: one way", 1, mapId);
            }

            int countBefore = owner.GateNodes.Count + partner.Entry.GateNodes.Count;
            var removed = NavGates.Remove(map, owner, gate);
            Count(removed.Removed == 2 && owner.GateNodes.Count + partner.Entry.GateNodes.Count == countBefore - 2 ? "remove: both sides" : "remove: not both sides", 1, mapId);
            var added = NavGates.Add(map, owner, target, p1, ToMap);
            Count(added.Ok && added.Added == 2 && NavGates.Status(map, owner, owner.GateNodes.Count - 1) == NavGateStatus.Paired ? "add: a pair" : $"add: {added.Error ?? added.Added.ToString()}", 1, mapId);

            var editIssues = NavigationValidator.MarkInherited(NavigationValidator.Validate(map), baseline);
            foreach (var issue in editIssues.Where(i => !i.Inherited && i.Severity == NavIssueSeverity.Error))
                Count($"edits: new {issue.Code}", 1, $"{mapId} {NavigationValidator.Describe(issue)}");

            edit.Restore();
            Count(before.AsSpan().SequenceEqual(nva.Write()) ? "edits: restore byte-identical" : "edits: restore differs", 1, mapId);
        }

        foreach (var (k, v) in c.OrderBy(x => x.Key)) Console.WriteLine($"{v,9}  {k}{(examples.TryGetValue(k, out var ex) ? "   e.g. " + ex : "")}");
        Console.WriteLine();
        foreach (var (k, l) in stats.OrderBy(x => x.Key))
        {
            l.Sort();
            float P(double q) => l[(int)Math.Min(l.Count - 1, Math.Floor(q * l.Count))];
            Console.WriteLine($"{k}: n {l.Count}, min {l[0]:0.###}, p10 {P(0.1):0.###}, p50 {P(0.5):0.###}, p90 {P(0.9):0.###}, p99 {P(0.99):0.###}, max {l[^1]:0.###}");
        }
    }
}
