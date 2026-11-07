using Andre.IO.VFS;
using Havok.hk2018;
using SoulsFormats;
using StudioCore.Editors.MapEditor;
using System.Numerics;
using System.Text.RegularExpressions;

// modeledit [regex] [max] : the Navigation tool's model edits (NavModelEdit) on game maps.
// - FaceDataIndex recomputed on the game's data changes nothing;
// - removing a model: the validator finds nothing new, nothing names or points at it, every gate still pairs, the
//   files write and read back, and restoring gives back the NVA and NVC files byte for byte;
// - duplicating a model into a free slot, moved: the validator finds nothing new, the copy's vertices, bounds and
//   mediator root box moved by the offset, and re-stitching the copy at no offset joins about as much as the original.
static class ModelEdit
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
        var rng = new Random(5);

        foreach (var mapId in maps)
        {
            var map = NavigationReader.Read(fs, mapId, profile, NavigationReader.NvcNamesFor(mapId, nvcNames));
            if (map?.Nva == null) continue;
            Count("maps");

            var nvaBytes = map.Nva.Write();
            var nvcBytes = map.Nvcs.ToDictionary(n => n.Name, n => n.Nvc.Write());

            NavModelEdit.RecomputeFaceDataIndex(map.Nva);
            Count(map.Nva.Write().AsSpan().SequenceEqual(nvaBytes) ? "game data: FaceDataIndex recompute changes nothing" : "game data: FaceDataIndex recompute changes the NVA", 1, mapId);

            int baseIssues = NavigationValidator.Validate(map, _ => null).Count(i => i.Severity == NavIssueSeverity.Error);
            var removable = NavModelEdit.Removable(map).ToList();
            var slots = NavModelEdit.FreeSlots(map).ToList();
            Count("models that can be removed", removable.Count);
            Count("free slots", slots.Count);
            Count(slots.Count > 0 ? "maps with a free slot" : "maps without a free slot");

            // User edge setups: where their boxes keep their translation
            foreach (var model in removable)
                foreach (var setups in model.NFile.Document.ObjectsOf<hkaiUserEdgeSetupArray>())
                    foreach (var s in setups.m_edgeSetups)
                    {
                        var aabb = model.NFile.Mesh.m_aabb;
                        bool In(Vector3 p) => p.X >= aabb.m_min.X - 5 && p.X <= aabb.m_max.X + 5 && p.Z >= aabb.m_min.Z - 5 && p.Z <= aabb.m_max.Z + 5;
                        var t = s.m_obbA.m_transform;
                        Count($"user edge setup: space {s.m_space}, translation in M41..43 {(In(new Vector3(t.M41, t.M42, t.M43)) ? "inside" : "outside")}, in M14..34 {(In(new Vector3(t.M14, t.M24, t.M34)) ? "inside" : "outside")}");
                    }

            if (removable.Count == 0) continue;

            // Remove
            foreach (var model in args.Length > 3 && args[3] == "all" ? removable : new List<NavmeshModel> { removable[rng.Next(removable.Count)] })
            {
                var state = new NavModelState(map);
                var names = new HashSet<int> { model.NEntry.NameID, model.OEntry.NameID };
                var positions = new HashSet<int> { map.Nva.Navmeshes.IndexOf(model.NEntry), map.Nva.Navmeshes.IndexOf(model.OEntry) };
                var result = NavModelEdit.Remove(map, model);
                if (!result.Ok) { Count($"remove refused: {result.Error}", 1, mapId); continue; }

                var after = Apply(map, result.Files);
                int issues = NavigationValidator.Validate(after, _ => null).Count(i => i.Severity == NavIssueSeverity.Error);
                Count(issues <= baseIssues ? "remove: no new validator errors" : "remove: new validator errors", 1, issues > baseIssues ? $"{mapId}: {baseIssues} -> {issues} {string.Join("; ", NavigationValidator.Validate(after, _ => null).Where(i => i.Severity == NavIssueSeverity.Error).Take(3).Select(i => i.Code + " " + string.Join(",", i.Args)))}" : null);

                bool named = NavStitchPlanner.Stored(map).Any(s => (s.MapA == mapId && names.Contains(s.MainNameID)) || (s.MapB == mapId && names.Contains(s.TargetNameID)))
                    || map.Nva.NodeBanks.Any(b => b.Faces.Any(f => names.Contains(f.CollisionID)));
                Count(named ? "remove: something still names the model" : "remove: nothing names the model", 1, named ? mapId : null);

                var list = map.Nva.Navmeshes;
                bool pointed = list.Any(e => e.GateNodes.Any(g => positions.Contains(g.ConnectedNavmeshIndex)));
                Count(pointed ? "remove: a gate still points at the model" : "remove: no gate points at the model", 1, pointed ? mapId : null);

                int pairs = 0, unpaired = 0;
                for (int i = 0; i < list.Count; i++)
                    for (int g = 0; g < list[i].GateNodes.Count; g++)
                    {
                        var gate = list[i].GateNodes[g];
                        if (gate.NodeSubID < 0 || gate.ConnectedNavmeshIndex >= list.Count || gate.NodeSubID >= list[gate.ConnectedNavmeshIndex].GateNodes.Count) continue;
                        var back = list[gate.ConnectedNavmeshIndex].GateNodes[gate.NodeSubID];
                        if (back.ConnectedNavmeshIndex == i && back.NodeSubID == g) pairs++; else unpaired++;
                    }
                Count("remove: gates that pair", pairs);
                Count("remove: gates that don't pair", unpaired, unpaired > 0 ? mapId : null);
                Count(list.All(e => e.GateNodes.All(g => g.NeighbourGateNodeCosts.Count == 16)) ? "remove: every gate keeps 16 costs" : "remove: a gate's costs changed length", 1);

                bool readsBack = result.Files.All(f => Havok.Files.HavokFile.Read(Havok.Files.HavokFile.Write(f.After), profile).ObjectsOf<hkaiNavMesh>().First().m_faces.Count == 0);
                bool nvaReads = NVA_ER.Read(map.Nva.Write()).Navmeshes.Count == list.Count;
                Count(readsBack && nvaReads ? "remove: NVA and emptied files write and read back" : "remove: write failed", 1, readsBack && nvaReads ? null : mapId);

                state.Restore();
                bool same = map.Nva.Write().AsSpan().SequenceEqual(nvaBytes) && map.Nvcs.All(n => n.Nvc.Write().AsSpan().SequenceEqual(nvcBytes[n.Name]));
                var diffs = new List<string>();
                if (!map.Nva.Write().AsSpan().SequenceEqual(nvaBytes)) diffs.Add("NVA");
                diffs.AddRange(map.Nvcs.Where(n => !n.Nvc.Write().AsSpan().SequenceEqual(nvcBytes[n.Name])).Select(n => n.Name));
                Count(same ? "remove: restore is byte-identical" : "remove: restore differs", 1, same ? null : $"{mapId}: {string.Join(", ", diffs)} (model {model.ModelID})");
            }

            // Duplicate, moved; then the same at no offset, re-stitched
            if (slots.Count > 0)
            {
                var state = new NavModelState(map);
                var source = removable[rng.Next(removable.Count)];
                var slot = slots[rng.Next(slots.Count)];
                var offset = new Vector3(37.5f, 2.25f, -12.75f);
                var result = NavModelEdit.Duplicate(map, source, slot, offset);
                if (!result.Ok) { Count($"duplicate refused: {result.Error}", 1, mapId); continue; }

                var after = Apply(map, result.Files);
                int issues = NavigationValidator.Validate(after, _ => null).Count(i => i.Severity == NavIssueSeverity.Error);
                Count(issues <= baseIssues ? "duplicate: no new validator errors" : "duplicate: new validator errors", 1, issues > baseIssues ? $"{mapId}: {baseIssues} -> {issues} {string.Join("; ", NavigationValidator.Validate(after, _ => null).Where(i => i.Severity == NavIssueSeverity.Error).Take(3).Select(i => i.Code + " " + string.Join(",", i.Args)))}" : null);

                foreach (var set in new[] { NavmeshSet.N, NavmeshSet.O })
                {
                    var src = source.File(set);
                    var copyDoc = result.Files.First(f => f.Name == NavigationMap.MeshName(map.NameBase, set, slot.ModelID)).After;
                    var reread = Havok.Files.HavokFile.Read(Havok.Files.HavokFile.Write(copyDoc), profile);
                    var copy = reread.ObjectsOf<hkaiNavMesh>().First();
                    bool moved = copy.m_vertices.Count == src.Mesh.m_vertices.Count
                        && copy.m_vertices.Zip(src.Mesh.m_vertices).All(p => Vector3.Distance(new Vector3(p.First.X, p.First.Y, p.First.Z), new Vector3(p.Second.X, p.Second.Y, p.Second.Z) + offset) < 1e-3f)
                        && Vector4.Distance(copy.m_aabb.m_min, src.Mesh.m_aabb.m_min + new Vector4(offset, 0)) < 1e-3f;
                    var d0 = src.Document.ObjectsOf<hkaiStaticTreeNavMeshQueryMediator>().FirstOrDefault()?.m_tree?.m_treePtr?.m_tree?.m_domain;
                    var d1 = reread.ObjectsOf<hkaiStaticTreeNavMeshQueryMediator>().FirstOrDefault()?.m_tree?.m_treePtr?.m_tree?.m_domain;
                    bool domain = d0 == null ? d1 == null : d1 != null && Vector4.Distance(d1.m_min, d0.m_min + new Vector4(offset, 0)) < 1e-3f;
                    Count(moved && domain ? $"duplicate {set}: copy moved by the offset, mediator root box too" : $"duplicate {set}: copy not moved right", 1, moved && domain ? null : mapId);
                    Count(copy.m_faces.Count == slot.Entry(set).FaceCount ? $"duplicate {set}: slot face count = copy" : $"duplicate {set}: face count differs", 1);
                }

                // A copy in place of the original joins about as much as the original does
                state.Restore();
                var inPlace = NavModelEdit.Duplicate(map, source, slot, Vector3.Zero);
                var placed = Apply(map, inPlace.Files);
                int originalConnections = NavStitchPlanner.Stored(map).Count(s => s.Nvc == null && s.MainNameID == source.NEntry.NameID);
                var plan = NavStitchPlanner.Plan(placed, NavmeshSet.N, null, new NavStitchParameters(), slot.NEntry.NameID, false);
                int copyJoins = plan.Added.Count(p => p.NameA == slot.NEntry.NameID && p.NameB != source.NEntry.NameID);
                Count("in-place copy: the original's in-map connections (n)", originalConnections);
                Count("in-place copy: a re-stitch would join, other than to the original (n)", copyJoins);

                state.Restore();
                bool same = map.Nva.Write().AsSpan().SequenceEqual(nvaBytes);
                Count(same ? "duplicate: restore is byte-identical" : "duplicate: restore differs", 1, same ? null : mapId);
            }
        }

        foreach (var (k, v) in c.OrderBy(x => x.Key)) Console.WriteLine($"{v,9}  {k}{(examples.TryGetValue(k, out var ex) ? "   e.g. " + ex : "")}");
    }

    /// <summary>The map with its files replaced as a model edit says.</summary>
    static NavigationMap Apply(NavigationMap map, IEnumerable<NavFileChange> changes)
    {
        var files = map.Files.ToDictionary(x => x.Key, x => x.Value);
        foreach (var change in changes)
        {
            if (change.After == null) files.Remove(change.Name);
            else files[change.Name] = NavigationReader.FromDocument(change.Name, change.After);
        }
        return new NavigationMap(map.MapId, map.Nva, files, map.Onav, map.Nvcs);
    }
}
