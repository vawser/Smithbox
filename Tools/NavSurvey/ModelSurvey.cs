using Andre.IO.VFS;
using SoulsFormats;
using StudioCore.Editors.MapEditor;
using System.Text.RegularExpressions;

// modelsurvey [regex] [max] : what refers to a navmesh entry by its position or NameID, and how placeholders and the
// entry order look, so models can be added and removed without breaking a reference.
static class ModelSurvey
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
        void Count(string key, string example = null) { c[key] = c.GetValueOrDefault(key) + 1; if (example != null) examples.TryAdd(key, example); }

        foreach (var mapId in maps)
        {
            var map = NavigationReader.Read(fs, mapId, profile, Array.Empty<string>());
            var nva = map?.Nva;
            if (nva == null) continue;
            var list = nva.Navmeshes;
            Count("maps");

            // Order: n half, o half, quarters
            var order = string.Concat(list.Select(e => NavigationMap.QuarterOf(e) != null ? 'q' : NavigationMap.SetOf(e) == NavmeshSet.N ? 'n' : 'o'));
            var shape = Regex.Replace(order, "(.)\\1*", "$1");
            Count($"order {shape}", mapId);
            var nModels = list.Where(e => NavigationMap.SetOf(e) == NavmeshSet.N && NavigationMap.QuarterOf(e) == null).Select(e => e.ModelID).ToList();
            Count(nModels.SequenceEqual(nModels.OrderBy(x => x)) ? "n half sorted by ModelID" : "n half not sorted by ModelID", mapId);

            // Placeholders
            foreach (var e in list)
            {
                bool hasFile = map.FileOf(e) != null;
                int faces = map.FileOf(e)?.FaceCount ?? -1;
                if (e.FaceCount == 0)
                    Count($"placeholder: Unk4C {e.Unk4C}, file {(hasFile ? (faces == 0 ? "empty" : "with faces") : "absent")}, set {NavigationMap.SetOf(e)}{(NavigationMap.QuarterOf(e) != null ? " quarter" : "")}, gate nodes {(e.GateNodes.Count == 0 ? "0" : "some")}", mapId);
                else if (e.Unk4C != 0)
                    Count($"entry with faces and Unk4C {e.Unk4C}", mapId);
            }

            // Gate nodes: what their navmesh index points at
            int gateTotal = list.Sum(e => e.GateNodes.Count);
            var positionOfNameId = list.Select((e, i) => (e.NameID, i)).ToDictionary(x => x.NameID, x => x.i);
            var connected = new HashSet<(int, int)>(nva.Connectors.Select(k => (k.MainNameID, k.TargetNameID)));

            for (int i = 0; i < list.Count; i++)
            {
                var owner = list[i];
                for (int g = 0; g < owner.GateNodes.Count; g++)
                {
                    var gate = owner.GateNodes[g];
                    int idx = gate.ConnectedNavmeshIndex;
                    Count("gate nodes");
                    if (idx < 0 || idx >= list.Count) { Count($"gate: index out of the list ({(idx < 0 ? "negative" : "past the end")})", $"{mapId} {owner.NameID} -> {idx}"); continue; }
                    var target = list[idx];
                    Count($"gate: owner {NavigationMap.SetOf(owner)} -> target {NavigationMap.SetOf(target)}");
                    if (idx == i) Count("gate: points at its own navmesh", mapId);
                    else Count(connected.Contains((owner.NameID, target.NameID)) ? "gate: target is connected to the owner" : "gate: target not connected to the owner", mapId);
                    Count(gate.NodeSubID >= 0 && gate.NodeSubID < target.GateNodes.Count ? "gate: sub ID < target's gate count" : "gate: sub ID other", $"{mapId} {gate.NodeSubID} of {target.GateNodes.Count}");
                    Count(gate.NeighbourGateNodeCosts.Count == owner.GateNodes.Count ? "gate: costs = owner's gate count" : gate.NeighbourGateNodeCosts.Count == gateTotal ? "gate: costs = map's gate count" : "gate: costs other", $"{mapId} {gate.NeighbourGateNodeCosts.Count} vs {owner.GateNodes.Count}");
                    if (gate.Unk14 != 0) Count("gate: Unk14 nonzero", mapId);
                    Count($"gate costs: owner gates {Math.Min(owner.GateNodes.Count, 6)}, costs {gate.NeighbourGateNodeCosts.Count}, beyond the owner's count {(gate.NeighbourGateNodeCosts.Skip(owner.GateNodes.Count).All(x => x == -1) ? "all -1" : "values")}, at its own index {(g < gate.NeighbourGateNodeCosts.Count ? gate.NeighbourGateNodeCosts[g].ToString() : "none")}");

                    // Does the target's gate with that sub ID point back at the owner?
                    if (gate.NodeSubID >= 0 && gate.NodeSubID < target.GateNodes.Count)
                    {
                        var back = target.GateNodes[gate.NodeSubID];
                        Count(back.ConnectedNavmeshIndex == i && back.NodeSubID == g ? "gate: the target's gate points back" : "gate: the target's gate doesn't point back", $"{mapId} {owner.NameID}#{g}");
                    }
                }
            }

            // The unknown sections
            foreach (var e in nva.Entries3) Count("entries3");
            foreach (var e in nva.Entries10)
            {
                Count("entries10");
                foreach (var (name, v) in new[] { ("Unk10", e.Unk10), ("Unk14", e.Unk14), ("Unk18", e.Unk18), ("Unk1C", e.Unk1C) })
                    Count($"entries10: {name} {(v == -1 ? "-1" : v >= 0 && v < list.Count ? "a list position" : positionOfNameId.ContainsKey(v) ? "a NameID" : "other")}", $"{mapId} {v} of {list.Count}");
            }
            foreach (var e in nva.Entries13)
                foreach (var v in e.Ints)
                    Count($"entries13 int: {(v == -1 ? "-1" : v == 0 ? "0" : v > 0 && v < list.Count ? "a list position" : positionOfNameId.ContainsKey(v) ? "a NameID" : "other")}", $"{mapId} {v} of {list.Count}");

            // Node bank faces name n navmeshes by NameID
            foreach (var bank in nva.NodeBanks)
                foreach (var f in bank.Faces)
                    Count(positionOfNameId.ContainsKey(f.CollisionID) ? "node bank face: NameID found" : "node bank face: NameID missing", mapId);
        }

        foreach (var (k, v) in c.OrderBy(x => x.Key)) Console.WriteLine($"{v,9}  {k}{(examples.TryGetValue(k, out var ex) ? "   e.g. " + ex : "")}");
    }
}
