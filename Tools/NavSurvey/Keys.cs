// Which maps the 32 bit halves of the NVC keys follow
static class Keys
{
    public static void Nvc(string dir)
    {
        var byMapHalf = new Dictionary<string, HashSet<string>>(); // value -> set of "map:role"
        void Add(uint v, string tag) { var k = v.ToString("X8"); if (!byMapHalf.TryGetValue(k, out var s)) byMapHalf[k] = s = new(); s.Add(tag); }
        foreach (var path in Directory.GetFiles(dir, "*.nvc"))
        {
            var b = File.ReadAllBytes(path);
            var n = Path.GetFileNameWithoutExtension(path);
            var a = n.Substring(0, 12); var c = n.Substring(13);
            int o = 16;
            for (int s = 0; s < 5; s++)
            {
                int type = Raw.I(b, o), size = Raw.I(b, o + 8), count = Raw.I(b, o + 12);
                if (type == 3) { Add((uint)Raw.I(b, o + 16), $"s3lo {a}|{c}"); Add((uint)Raw.I(b, o + 20), $"s3hi {a}|{c}"); }
                if (type == 0) for (int i = 0; i < count; i++) { Add((uint)Raw.I(b, o + 16 + i * 32), $"s0lo {a}|{c}"); Add((uint)Raw.I(b, o + 20 + i * 32), $"s0hi {a}|{c}"); }
                o += 16 + size;
            }
        }
        var multi = byMapHalf.Where(x => x.Value.Count > 1).ToList();
        Console.WriteLine($"{byMapHalf.Count} distinct halves, {multi.Count} repeat");
        foreach (var (k, v) in multi.Take(15)) Console.WriteLine($"{k}: {string.Join("; ", v.Take(6))}");
    }
}
