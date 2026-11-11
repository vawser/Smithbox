using SoulsFormats;

// Matches every 4-byte-aligned s32 field of BHV records against AC6 regulation param row IDs.
public static class ParamMatch
{
    public static void Run(string root, string regulation, int version)
    {
        var reg = SFUtil.DecryptAC6Regulation(regulation);
        var ids = new Dictionary<string, HashSet<int>>();
        foreach (var f in reg.Files.Where(f => f.Name.EndsWith(".param")))
        {
            try { var p = PARAM.Read(f.Bytes); ids[Path.GetFileNameWithoutExtension(f.Name)] = p.Rows.Select(r => r.ID).ToHashSet(); }
            catch { }
        }
        Console.WriteLine($"{ids.Count} params");

        var groups = new Dictionary<string, List<byte[]>>();
        void Add(string k, byte[] d) { if (!groups.TryGetValue(k, out var l)) groups[k] = l = new(); l.Add(d); }
        foreach (var path in Directory.GetFiles(root, "*.bhv", SearchOption.AllDirectories))
        {
            var bhv = BHV.Read(File.ReadAllBytes(path));
            if (bhv.Version != version) continue;
            var k = bhv.Kind;
            foreach (var s in bhv.States)
            {
                Add($"{k} body", s.Body);
                if (s.Data.Length > 0) Add($"{k} statedata kind{s.Kind} len{s.Data.Length:X}", s.Data);
                foreach (var t in s.Transitions)
                {
                    Add($"params type{t.Params[1]}", t.Params);
                    foreach (var c in t.Conditions) if (c.Data.Length > 0) Add($"cond{c.Type} len{c.Data.Length:X}", c.Data);
                }
            }
            foreach (var b in bhv.Animations) Add($"{k} B", b);
            foreach (var c in bhv.StructCs) Add($"{k} C len{c.Length:X}", c);
            foreach (var d in bhv.StructDs) foreach (var a in d.StructDAs) Add($"{k} DA", a);
            if (bhv.Preamble.Length > 0) Add($"{k} preamble", bhv.Preamble);
        }

        foreach (var (key, recs) in groups.OrderBy(g => g.Key))
        {
            int len = recs.GroupBy(r => r.Length).OrderByDescending(g => g.Count()).First().Key;
            var rs = recs.Where(r => r.Length == len).ToList();
            for (int o = 0; o + 4 <= len; o += 4)
            {
                var vals = rs.Select(r => BitConverter.ToInt32(r, o)).ToList();
                var distinct = vals.Where(v => v > 0 && v != 65535).Distinct().ToList();
                if (distinct.Count < 3 || distinct.Max() < 1000) continue;
                var best = ids.Select(p => (p.Key, Hit: distinct.Count(v => p.Value.Contains(v)) / (double)distinct.Count))
                    .OrderByDescending(x => x.Hit).Take(3).ToList();
                if (best[0].Hit >= (Environment.GetEnvironmentVariable("BHV_MIN") is string m ? double.Parse(m) : 0.8))
                    Console.WriteLine($"{key,-34} +{o:X2} n={distinct.Count,-5} uses={vals.Count(v => v > 0 && v != 65535),-6} {string.Join("  ", best.Select(b => $"{b.Key}({b.Hit:P0})"))}  e.g. {string.Join(",", distinct.Take(5))}");
            }
        }
    }
}
