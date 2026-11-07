using System.Buffers.Binary;

// Format-agnostic probes over the extracted files, used to decode the layouts before SoulsFormats had them
static class Raw
{
    static readonly Dictionary<string, Dictionary<string, int>> agg = new();
    public static void Count(string key, object value)
    {
        if (!agg.TryGetValue(key, out var d)) agg[key] = d = new();
        var s = value?.ToString() ?? "null";
        d[s] = d.GetValueOrDefault(s) + 1;
    }
    public static void Print(int top = 12)
    {
        foreach (var (k, d) in agg.OrderBy(x => x.Key))
            Console.WriteLine($"{k} [{d.Count} distinct]: {string.Join(", ", d.OrderByDescending(x => x.Value).Take(top).Select(x => $"{x.Key} x{x.Value}"))}");
        agg.Clear();
    }
    public static int I(byte[] b, int o) => BinaryPrimitives.ReadInt32LittleEndian(b.AsSpan(o));
    public static long L(byte[] b, int o) => BinaryPrimitives.ReadInt64LittleEndian(b.AsSpan(o));
    public static float F(byte[] b, int o) => BitConverter.ToSingle(b, o);

    public static void Nvc(string dir)
    {
        foreach (var path in Directory.GetFiles(dir, "*.nvc"))
        {
            var b = File.ReadAllBytes(path);
            var name = Path.GetFileName(path);
            Count("magic", System.Text.Encoding.ASCII.GetString(b, 0, 4));
            Count("version", I(b, 4));
            Count("size==len", I(b, 8) == b.Length);
            Count("sectionCount", I(b, 12));
            int o = 16;
            var order = new List<int>();
            var sec = new Dictionary<int, (int unk, int size, int count, int off)>();
            for (int s = 0; s < I(b, 12); s++)
            {
                int type = I(b, o), unk = I(b, o + 4), size = I(b, o + 8), count = I(b, o + 12);
                order.Add(type);
                sec[type] = (unk, size, count, o + 16);
                Count($"sec{type}.unk", unk);
                Count($"sec{type}.stride", count == 0 ? "c0" : size % count == 0 ? (size / count).ToString() : $"odd {size}/{count}");
                if (size == 0) Count($"sec{type}.emptyCount", count);
                o += 16 + size;
            }
            Count("end==len", o == b.Length);
            Count("order", string.Join(",", order));

            // Section 3: one 16 byte record
            var s3 = sec[3];
            Count("sec3.count", s3.count);
            Count("sec3.hi8", $"{I(b, s3.off + 8)},{I(b, s3.off + 12)}");
            // Section 0: 32 byte groups
            var s0 = sec[0];
            int total = 0, prevEnd = 0;
            for (int i = 0; i < s0.count; i++)
            {
                int e = s0.off + i * 32;
                int cnt = I(b, e + 8), start = I(b, e + 16);
                Count("sec0.+C", I(b, e + 12)); Count("sec0.+14", I(b, e + 20)); Count("sec0.+18", I(b, e + 24)); Count("sec0.+1C", I(b, e + 28));
                Count("sec0.start==prevEnd", start == prevEnd);
                prevEnd = start + cnt; total += cnt;
            }
            Count("sec0.count", s0.count);
            Count("sec0.sum==sec1.count", total == sec[1].count);
            // Section 1: 16 byte connections
            var s1 = sec[1];
            for (int i = 0; i < s1.count; i++)
            {
                int e = s1.off + i * 16;
                Count("sec1.edge-3face(a)", Math.Clamp(I(b, e + 4) - 3 * I(b, e), -2, 20));
                Count("sec1.edge-3face(b)", Math.Clamp(I(b, e + 12) - 3 * I(b, e + 8), -2, 20));
            }
            foreach (var t in new[] { 2, 4 }) if (sec.ContainsKey(t) && sec[t].size > 0) { Count($"sec{t}.nonEmpty", name); }
        }
        Print();
    }

    public static void Onav(string dir) { if (File.Exists(dir)) { RawOnav.Map(dir); RawOnav3.Objects(dir); } else { RawOnav5.Survey(dir); } }
}
