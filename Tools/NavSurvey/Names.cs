using Andre.IO.VFS;
using StudioCore.Editors.MapEditor;
using System.Text.RegularExpressions;

// names [regex] : the file name the Map Editor's navmesh drawing asks for (from the NVA entry alone) against the
// file the binder has for it (NavigationMap, which reads the binder's names)
static class Names
{
    public static void Run(VirtualFileSystem fs, string dict, string[] args)
    {
        var re = new Regex(args.Length > 1 ? args[1] : ".");
        var maps = Regex.Matches(dict, @"""/map/m\d\d/(m\d\d_\d\d_\d\d_\d\d)/\1\.nva\.dcx""").Select(m => m.Groups[1].Value)
            .Distinct().Where(m => re.IsMatch(m)).ToList();
        var profile = Havok.HavokProfile.FromGame("ER");
        var counts = new Dictionary<string, int>();
        void C(string k) => counts[k] = counts.GetValueOrDefault(k) + 1;
        int shown = 0;

        Parallel.ForEach(maps, new ParallelOptions { MaxDegreeOfParallelism = 8 }, mapId =>
        {
            var map = NavigationReader.Read(fs, mapId, profile, Array.Empty<string>());
            if (map == null) return;

            foreach (var entry in map.Nva.Navmeshes)
            {
                var fromEntry = NavigationMap.FileNameFromEntry(mapId, entry);
                var fromBinder = map.FileNameOf(entry);
                bool exists = map.Files.ContainsKey(fromEntry);
                // What the drawing loaded before: always the n file, named after the map
                var old = NavigationMap.MeshName(mapId, NavmeshSet.N, entry.ModelID);
                bool oldRight = old == fromBinder;

                lock (counts)
                {
                    C(fromEntry == fromBinder ? "same as the binder's" : "DIFFERENT from the binder's");
                    if (entry.FaceCount > 0) C(exists ? "has faces: file found" : "has faces: FILE MISSING");
                    if (entry.FaceCount > 0 && exists && map.Files[fromEntry].FaceCount == entry.FaceCount) C("has faces: face count matches");
                    C(oldRight ? "old name was right" : "old name was wrong");
                    if (fromEntry != fromBinder && shown++ < 10) Console.WriteLine($"{mapId}: {(uint)entry.NameID} entry {fromEntry} binder {fromBinder}");
                }
            }
        });

        foreach (var (k, v) in counts.OrderBy(x => x.Key)) Console.WriteLine($"{k}: {v}");
    }
}
