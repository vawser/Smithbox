using Andre.IO.VFS;
using Havok.hk2018;
using SoulsFormats;
using StudioCore.Editors.MapEditor;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;

// validate [regex] [max] [verbose] : builds the Map Editor's NavigationMap for each map from the game files, runs its
// NavigationValidator, and checks that the remap service leaves files byte-identical for an identity and an undone shift
static class Validate
{
    // The Map Editor's reader, so this checks what Smithbox builds
    public static NavigationMap Build(VirtualFileSystem fs, string map, Havok.HavokProfile profile, Dictionary<string, List<string>> nvcNames)
        => NavigationReader.Read(fs, map, profile, nvcNames.GetValueOrDefault(map) ?? new());

    public static void Run(VirtualFileSystem fs, string dict, string[] args)
    {
        var re = new Regex(args.Length > 1 ? args[1] : ".");
        int max = args.Length > 2 ? int.Parse(args[2]) : int.MaxValue;
        bool verbose = args.Length > 3 && args[3] == "verbose";
        var maps = Regex.Matches(dict, @"""/map/m\d\d/(m\d\d_\d\d_\d\d_\d\d)/\1\.nva\.dcx""").Select(m => m.Groups[1].Value)
            .Distinct().Where(m => re.IsMatch(m)).Take(max).ToList();

        // NVC files by the maps they name
        var nvcNames = new Dictionary<string, List<string>>();
        foreach (var name in Regex.Matches(dict, @"""/map/nvc/(m\d\d_\d\d_\d\d_\d\d_m\d\d_\d\d_\d\d_\d\d)\.nvc\.dcx""").Select(m => m.Groups[1].Value).Distinct())
        {
            foreach (var m in new[] { name.Substring(0, 12), name.Substring(13) })
                (nvcNames.TryGetValue(m, out var l) ? l : nvcNames[m] = new()).Add(name);
        }

        var profile = Havok.HavokProfile.FromGame("ER");
        var built = new ConcurrentDictionary<string, NavigationMap>();
        Parallel.ForEach(maps, new ParallelOptions { MaxDegreeOfParallelism = 8 }, map =>
        {
            try { var m = Build(fs, map, profile, nvcNames); if (m != null) built[map] = m; }
            catch (Exception e) { Console.WriteLine($"{map}: build failed: {e.Message}"); }
        });
        Console.WriteLine($"{built.Count} of {maps.Count} maps built");

        var counts = new ConcurrentDictionary<string, int>();
        var examples = new ConcurrentDictionary<string, string>();
        int remapOk = 0, remapBad = 0;
        foreach (var map in built.Values.OrderBy(m => m.MapId))
        {
            var issues = NavigationValidator.Validate(map, built.GetValueOrDefault);
            foreach (var issue in issues)
            {
                var key = $"{issue.Severity} {issue.Code}";
                counts.AddOrUpdate(key, 1, (_, c) => c + 1);
                examples.TryAdd(key, $"{map.MapId}: {NavigationValidator.Describe(issue)}");
                if (verbose) Console.WriteLine($"{map.MapId}: {NavigationValidator.Describe(issue)}");
            }
            counts.AddOrUpdate(issues.Any(i => i.Severity == NavIssueSeverity.Error) ? "maps with errors" : "maps without errors", 1, (_, c) => c + 1);

            if (CheckRemap(map)) remapOk++; else remapBad++;
        }

        foreach (var (k, v) in counts.OrderBy(x => x.Key)) Console.WriteLine($"{k}: {v}{(examples.TryGetValue(k, out var e) ? $"   e.g. {e}" : "")}");
        Console.WriteLine($"remap round trip: {remapOk} maps identical, {remapBad} changed");
        Console.WriteLine($"connections checked: {built.Values.Sum(m => NavigationRemap.Connections(m).Count())} (NVC ones counted from both maps)");

        // Breaking each invariant must be caught
        var ordered = built.Values.OrderBy(m => m.MapId).ToList();
        var inMap = ordered.FirstOrDefault(m => m.Nva.Connectors.Any(c => c.NavMeshConnections.Count > 0) && m.Models.Any(x => x.NFile?.FaceCount > 0));
        var crossMap = ordered.FirstOrDefault(m => m.Nvcs.Any(n => n.MapA == m.MapId && n.Nvc.Connectors.Any(c => c.NavMeshConnections.Count > 0)));
        SelfTest(fs, inMap, crossMap, profile, nvcNames, built);
    }

    static void SelfTest(VirtualFileSystem fs, NavigationMap inMap, NavigationMap crossMap, Havok.HavokProfile profile,
        Dictionary<string, List<string>> nvcNames, ConcurrentDictionary<string, NavigationMap> built)
    {
        Console.WriteLine($"self test on {inMap?.MapId ?? "-"} (in-map) and {crossMap?.MapId ?? "-"} (NVC):");
        // Each break is checked as the Map Editor reports it: against the unbroken (vanilla) map, so it must be new
        void Expect(NavigationMap original, string what, Action<NavigationMap> breakIt, NavIssueCode code)
        {
            if (original == null) { Console.WriteLine($"  skipped  {what}: no map in the selection has the data"); return; }
            var vanilla = NavigationValidator.Validate(original, built.GetValueOrDefault);
            var map = Build(fs, original.MapId, profile, nvcNames);
            breakIt(map);
            var issues = NavigationValidator.MarkInherited(NavigationValidator.Validate(map, built.GetValueOrDefault), vanilla);
            var found = issues.Any(i => i.Code == code && !i.Inherited);
            Console.WriteLine($"  {(found ? "caught " : "MISSED ")} {what} -> {code}");
        }

        // A map whose vanilla data has issues has none of its own against itself
        foreach (var name in new[] { "m60_51_36_00", "m61_44_52_00" })
        {
            if (!built.TryGetValue(name, out var stale)) continue;
            var vanilla = NavigationValidator.Validate(stale, built.GetValueOrDefault);
            var fresh = NavigationValidator.MarkInherited(NavigationValidator.Validate(Build(fs, name, profile, nvcNames), built.GetValueOrDefault), vanilla);
            Console.WriteLine($"  {name}: {vanilla.Count} vanilla issues, {fresh.Count(i => !i.Inherited)} new against the baseline (expect 0)");
        }
        static NVA_ER.NavMeshConnection FirstNva(NavigationMap m) => m.Nva.Connectors.First(c => c.NavMeshConnections.Count > 0).NavMeshConnections[0];
        static NvcFile FirstNvc(NavigationMap m) => m.Nvcs.First(n => n.MapA == m.MapId && n.Nvc.Connectors.Any(c => c.NavMeshConnections.Count > 0));
        static NVA_ER.NavMeshConnection FirstNvcConnection(NavigationMap m) => FirstNvc(m).Nvc.Connectors.First(c => c.NavMeshConnections.Count > 0).NavMeshConnections[0];

        Expect(inMap, "connection face out of range", m => FirstNva(m).FaceIndex = 999999, NavIssueCode.ConnectionFaceRange);
        Expect(inMap, "connection edge off its face", m => FirstNva(m).EdgeIndex += 50000, NavIssueCode.ConnectionEdgeNotOnFace);
        Expect(inMap, "connector to a missing navmesh", m => m.Nva.Connectors[0].TargetNameID = 12345, NavIssueCode.ConnectorNavmeshMissing);
        Expect(inMap, "FaceCount", m => m.Models.First(x => x.NFile?.FaceCount > 0).NEntry.FaceCount += 1, NavIssueCode.FaceCountMismatch);
        Expect(inMap, "FaceDataIndex", m => m.Nva.Navmeshes[^1].FaceDataIndex += 1, NavIssueCode.FaceDataIndex);
        Expect(inMap, "NameID", m => m.Nva.Navmeshes[0].NameID += 7, NavIssueCode.NameIdRule);
        Expect(inMap, "o entry without its n entry", m => m.Nva.Navmeshes.First(n => NavigationMap.SetOf(n) == NavmeshSet.O).ModelID += 1, NavIssueCode.OEntryUnpaired);
        Expect(crossMap, "NVC face out of range", m => FirstNvcConnection(m).FaceIndex = 999999, NavIssueCode.ConnectionFaceRange);
        Expect(crossMap, "NVC edge off its face", m => FirstNvcConnection(m).EdgeIndex += 50000, NavIssueCode.ConnectionEdgeNotOnFace);
        Expect(crossMap, "NVC map UID", m => FirstNvc(m).Nvc.MapUID ^= 1, NavIssueCode.NvcMapUid);
    }

    /// <summary>
    /// Identity, then shifting every navmesh's indices by 1000 and back, must leave the NVA and NVC bytes as they were
    /// </summary>
    static bool CheckRemap(NavigationMap map)
    {
        var before = Snapshot(map);
        foreach (var n in map.Nva.Navmeshes)
        {
            var id = NavigationRemap.RemapNavmesh(map, n.NameID, f => f, e => e);
            if (id.Changed != 0 || id.Dropped != 0) return false;
            NavigationRemap.RemapNavmesh(map, n.NameID, f => f + 1000, e => e + 1000);
            NavigationRemap.RemapNavmesh(map, n.NameID, f => f - 1000, e => e - 1000);
        }
        return Snapshot(map).SequenceEqual(before);
    }

    static List<byte[]> Snapshot(NavigationMap map) => new[] { map.Nva.Write() }.Concat(map.Nvcs.Select(n => n.Nvc.Write())).ToList();
}

static class SequenceEqualExt
{
    public static bool SequenceEqual(this List<byte[]> a, List<byte[]> b) => a.Count == b.Count && a.Zip(b).All(z => z.First.AsSpan().SequenceEqual(z.Second));
}
