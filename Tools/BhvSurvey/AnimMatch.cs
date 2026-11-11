using SoulsFormats;

// Tests s32 fields of BHV records against the TAE animation IDs of the same character's anibnds.
public static class AnimMatch
{
    static readonly Dictionary<HashSet<long>, HashSet<long>> shortCache = new();
    static readonly Dictionary<string, (int Uses, int Short, int Full, HashSet<int> Distinct)> stats = new();

    public static HashSet<long> LoadAnims(Andre.IO.VFS.VirtualFileSystem fs, IEnumerable<string> anibnds)
    {
        var set = new HashSet<long>();
        foreach (var p in anibnds)
        {
            try
            {
                var bnd = BND4.Read(fs.ReadFile(p).Value);
                foreach (var f in bnd.Files.Where(f => f.Name.EndsWith(".tae", StringComparison.OrdinalIgnoreCase)))
                {
                    try
                    {
                        var tae = TAE.Read(f.Bytes);
                        foreach (var a in tae.Animations) { set.Add(a.ID); set.Add(tae.ID * 1_000_000L + a.ID % 1_000_000); }
                    }
                    catch { }
                }
            }
            catch { }
        }
        return set;
    }

    public static void Add(BHV bhv, HashSet<long> anims)
    {
        if (anims.Count == 0) return;
        if (!shortCache.TryGetValue(anims, out var shortSet)) shortCache[anims] = shortSet = anims.Select(a => a % 1_000_000).ToHashSet();
        void Rec(string key, byte[] d)
        {
            for (int o = 0; o + 4 <= d.Length; o += 4)
            {
                int v = BitConverter.ToInt32(d, o);
                if (v <= 0 || v == 65535) continue;
                var k = $"{key} +{o:X2}";
                (int Uses, int Short, int Full, HashSet<int> Distinct) e = stats.TryGetValue(k, out var x) ? x : (0, 0, 0, new HashSet<int>());
                e.Uses++; e.Distinct.Add(v);
                if (anims.Contains(v)) e.Full++;
                if (shortSet.Contains(v)) e.Short++;
                stats[k] = e;
            }
        }
        foreach (var b in bhv.Animations) Rec($"v{bhv.Version} {bhv.Kind} B", b);
        foreach (var d in bhv.StructDs) foreach (var a in d.StructDAs) Rec($"v{bhv.Version} {bhv.Kind} DA", a);
        foreach (var s in bhv.States) Rec($"v{bhv.Version} {bhv.Kind} body", s.Body);
    }

    public static void Report()
    {
        foreach (var (k, e) in stats.Where(s => s.Value.Distinct.Count >= 5 && s.Value.Distinct.Max() >= 100).OrderBy(s => s.Key))
        {
            double sh = e.Short / (double)e.Uses, fu = e.Full / (double)e.Uses;
            if (sh >= 0.5 || fu >= 0.5)
                Console.WriteLine($"{k,-22} uses={e.Uses,-6} distinct={e.Distinct.Count,-5} anim(full)={fu:P0} anim(id%1e6)={sh:P0}  e.g. {string.Join(",", e.Distinct.Take(6))}");
        }
    }
}
