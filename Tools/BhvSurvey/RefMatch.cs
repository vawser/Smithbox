using SoulsFormats;

// Tests every s16 / u8 field of BHV records as an index into a list of the same file (states, B, C, D, strings).
// A real index is always below that list's count in its own file and reaches close to the count in some files.
public static class RefMatch
{
    record Rec(byte[] Bytes, Dictionary<string, int> Counts);

    public static void Run(string root, int version)
    {
        var groups = new Dictionary<string, List<Rec>>();
        void Add(string k, byte[] d, Dictionary<string, int> c) { if (!groups.TryGetValue(k, out var l)) groups[k] = l = new(); l.Add(new(d, c)); }
        foreach (var path in Directory.GetFiles(root, "*.bhv", SearchOption.AllDirectories))
        {
            var bhv = BHV.Read(File.ReadAllBytes(path));
            if (bhv.Version != version) continue;
            var counts = new Dictionary<string, int>
            {
                ["states"] = bhv.States.Count, ["B"] = bhv.Animations.Count, ["C"] = bhv.StructCs.Count,
                ["D"] = bhv.StructDs.Count, ["strings"] = bhv.Strings?.Count ?? 0,
            };
            var k = bhv.Kind;
            if (bhv.Preamble.Length > 0) Add($"{k} preamble", bhv.Preamble, counts);
            foreach (var s in bhv.States)
            {
                Add($"{k} body", s.Body, counts);
                if (s.Data.Length > 0) Add($"{k} statedata", s.Data, counts);
                foreach (var t in s.Transitions)
                {
                    Add($"params type{t.Params[1]}", t.Params, counts);
                    foreach (var c in t.Conditions) if (c.Data.Length > 0) Add($"cond{c.Type,-3} len{c.Data.Length:X}", c.Data, counts);
                }
            }
            foreach (var b in bhv.Animations) Add($"{k} B", b, counts);
            foreach (var d in bhv.StructDs) foreach (var a in d.StructDAs) Add($"{k} DA", a, counts);
        }

        var lists = new[] { "states", "B", "C", "D", "strings" };
        foreach (var (key, recs) in groups.OrderBy(g => g.Key))
        {
            int len = recs.GroupBy(r => r.Bytes.Length).OrderByDescending(g => g.Count()).First().Key;
            var rs = recs.Where(r => r.Bytes.Length == len).ToList();
            // statedata is a variable list: test every s16 in it
            bool whole = key.EndsWith("statedata");
            for (int o = 0; o + 2 <= (whole ? 2 : len); o += 2)
            {
                var vals = whole
                    ? recs.SelectMany(r => Enumerable.Range(0, r.Bytes.Length / 2).Select(i => ((int)BitConverter.ToInt16(r.Bytes, i * 2), r.Counts))).ToList()
                    : rs.Select(r => ((int)BitConverter.ToInt16(r.Bytes, o), r.Counts)).ToList();
                var used = vals.Where(v => v.Item1 >= 0).ToList();
                if (used.Count < 20 || used.Select(v => v.Item1).Distinct().Count() < 8) continue;
                foreach (var l in lists)
                {
                    double valid = used.Count(v => v.Item1 < v.Counts[l]) / (double)used.Count;
                    if (valid < 0.999) continue;
                    double reach = used.Max(v => v.Counts[l] == 0 ? 0 : (v.Item1 + 1) / (double)v.Counts[l]);
                    // a field that is always below even a much smaller list is not discriminating
                    var smaller = lists.Where(x => x != l).Select(x => (x, used.Count(v => v.Item1 < v.Counts[x]) / (double)used.Count)).Where(x => x.Item2 >= 0.999).Select(x => x.x);
                    Console.WriteLine($"{key,-24} {(whole ? "s16[]" : $"+{o:X2} s16"),-9} -> {l,-7} uses={used.Count,-6} distinct={used.Select(v => v.Item1).Distinct().Count(),-5} reach={reach:P0}  also<{string.Join(",", smaller)}");
                }
            }
        }
    }
}
